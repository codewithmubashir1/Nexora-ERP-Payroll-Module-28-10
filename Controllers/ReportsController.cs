using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ERP.Identity.Controllers
{
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}")]
    public class ReportsController : Controller
    {
        private const int MaximumRows = 10000;
        private readonly ApplicationDbContext _db;
        private readonly ICompanyInfoService _companyInfoService;

        public ReportsController(ApplicationDbContext db, ICompanyInfoService companyInfoService)
        {
            _db = db;
            _companyInfoService = companyInfoService;
        }

        public IActionResult Index() => View(new ReportsViewModel { ReportTypes = GetReportTypes() });

        [HttpGet("GetData")]
        public async Task<IActionResult> GetData(string entity, DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date) return BadRequest("From date must be on or before To date.");
            var definition = FindDefinition(entity);
            if (definition == null) return BadRequest("Choose a supported report entity.");

            var data = await LoadReportAsync(definition, fromDate.Date, toDate.Date);
            return Json(data);
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export(string entity, DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date) return BadRequest("From date must be on or before To date.");
            var definition = FindDefinition(entity);
            if (definition == null) return BadRequest("Choose a supported report entity.");

            var data = await LoadReportAsync(definition, fromDate.Date, toDate.Date);
            var bytes = CsvUtility.WriteWithHeader(data.Columns, data.Rows);
            return File(bytes, "text/csv", $"{definition.EntityType.Name.ToLowerInvariant()}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv");
        }

        [HttpGet("Print")]
        public async Task<IActionResult> Print(string entity, DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date) return BadRequest("From date must be on or before To date.");
            var definition = FindDefinition(entity);
            if (definition == null) return BadRequest("Choose a supported report entity.");

            var data = await LoadReportAsync(definition, fromDate.Date, toDate.Date);
            var report = new PrintDocumentViewModel
            {
                DocumentTypeTitle = $"{definition.Label} Report",
                DocumentNumberLabel = "PERIOD",
                DocumentNumber = $"{fromDate:dd-MM-yyyy} to {toDate:dd-MM-yyyy}",
                ExtraRefs = new() { ("RECORDS", data.Rows.Count.ToString(CultureInfo.InvariantCulture)) },
                PartyLabel = "REPORT",
                PartyName = definition.Label,
                Columns = data.Columns,
                Rows = data.Rows,
                Totals = new() { ("Records", data.Rows.Count.ToString(CultureInfo.InvariantCulture), true) },
                AdditionalNotes = $"Date field: {definition.DateProperty.Name}. Maximum displayed rows: {MaximumRows}.",
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };
            return View("~/Views/Shared/_PrintDocument.cshtml", report);
        }

        private List<ReportTypeOption> GetReportTypes() => _db.Model.GetEntityTypes()
            .Where(type => type.GetTableName() != null && !type.IsOwned() &&
                type.ClrType.Namespace?.StartsWith("ERP.Identity.Models", StringComparison.Ordinal) == true &&
                type.ClrType.Name != "ApplicationUser")
            .Select(type => new { Type = type, Date = FindDateProperty(type) })
            .Where(item => item.Date != null)
            .OrderBy(item => item.Type.ClrType.Name)
            .Select(item => new ReportTypeOption(
                item.Type.ClrType.FullName!,
                Regex.Replace(item.Type.ClrType.Name, "([a-z0-9])([A-Z])", "$1 $2"),
                item.Date!.Name))
            .ToList();

        private ReportDefinition? FindDefinition(string key)
        {
            var type = _db.Model.GetEntityTypes().FirstOrDefault(item => item.ClrType.FullName == key &&
                item.GetTableName() != null && !item.IsOwned() && item.ClrType.Name != "ApplicationUser");
            if (type == null || type.ClrType.Namespace?.StartsWith("ERP.Identity.Models", StringComparison.Ordinal) != true)
                return null;

            var dateProperty = FindDateProperty(type);
            if (dateProperty?.PropertyInfo == null) return null;
            var properties = type.GetProperties()
                .Select(property => property.PropertyInfo)
                .Where(property => property != null && IsReportScalar(property.PropertyType))
                .Cast<PropertyInfo>()
                .Take(24)
                .ToList();
            return new ReportDefinition(type.ClrType, dateProperty.PropertyInfo, properties,
                Regex.Replace(type.ClrType.Name, "([a-z0-9])([A-Z])", "$1 $2"));
        }

        private async Task<ReportData> LoadReportAsync(ReportDefinition definition, DateTime fromDate, DateTime toDate)
        {
            var setMethod = typeof(DbContext).GetMethods()
                .First(method => method.Name == nameof(DbContext.Set) && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
                .MakeGenericMethod(definition.EntityType);
            var query = (IQueryable)setMethod.Invoke(_db, null)!;
            query = ApplyDateRange(query, definition.EntityType, definition.DateProperty, fromDate, toDate);

            var takeMethod = typeof(Queryable).GetMethods()
                .First(method => method.Name == nameof(Queryable.Take) && method.IsGenericMethodDefinition && method.GetParameters().Length == 2 &&
                    method.GetParameters()[1].ParameterType == typeof(int))
                .MakeGenericMethod(definition.EntityType);
            query = (IQueryable)takeMethod.Invoke(null, new object[] { query, MaximumRows })!;

            var toListMethod = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                .First(method => method.Name == nameof(EntityFrameworkQueryableExtensions.ToListAsync) && method.IsGenericMethodDefinition && method.GetParameters().Length == 2)
                .MakeGenericMethod(definition.EntityType);
            var task = (Task)toListMethod.Invoke(null, new object[] { query, HttpContext.RequestAborted })!;
            await task;
            var entities = (IEnumerable)task.GetType().GetProperty("Result")!.GetValue(task)!;

            var columns = definition.Properties.Select(GetPropertyLabel).ToList();
            var rows = entities.Cast<object>().Select(entity => definition.Properties
                .Select(property => FormatValue(property.GetValue(entity)))
                .ToList()).ToList();
            return new ReportData(columns, rows);
        }

        private static IQueryable ApplyDateRange(IQueryable query, Type entityType, PropertyInfo dateProperty, DateTime from, DateTime to)
        {
            var parameter = Expression.Parameter(entityType, "entity");
            Expression value = Expression.Property(parameter, dateProperty);
            Expression? condition = null;
            if (Nullable.GetUnderlyingType(dateProperty.PropertyType) == typeof(DateTime))
            {
                var hasValue = Expression.Property(value, "HasValue");
                value = Expression.Property(value, "Value");
                condition = hasValue;
            }

            var lower = Expression.GreaterThanOrEqual(value, Expression.Constant(from, typeof(DateTime)));
            var upper = Expression.LessThan(value, Expression.Constant(to.AddDays(1), typeof(DateTime)));
            condition = condition == null ? Expression.AndAlso(lower, upper) : Expression.AndAlso(condition, Expression.AndAlso(lower, upper));
            var predicate = Expression.Lambda(condition, parameter);
            var whereMethod = typeof(Queryable).GetMethods().First(method => method.Name == nameof(Queryable.Where) &&
                method.IsGenericMethodDefinition && method.GetParameters().Length == 2 &&
                method.GetParameters()[1].ParameterType.GetGenericArguments()[0].IsGenericType &&
                method.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<,>) &&
                method.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(entityType);
            return (IQueryable)whereMethod.Invoke(null, new object[] { query, predicate })!;
        }

        private static IProperty? FindDateProperty(IEntityType entityType)
        {
            var candidates = new[] { "Date", "InvoiceDate", "VoucherDate", "AttendanceDate", "SODate", "PODate", "EffectiveDate", "CreatedOn" };
            return candidates.Select(entityType.FindProperty).FirstOrDefault(property => property != null && IsDate(property.ClrType)) ??
                entityType.GetProperties().FirstOrDefault(property => IsDate(property.ClrType));
        }

        private static bool IsDate(Type type) => type == typeof(DateTime) || Nullable.GetUnderlyingType(type) == typeof(DateTime);

        private static bool IsReportScalar(Type type) =>
            type.IsEnum || type == typeof(string) || type == typeof(bool) || type == typeof(DateTime) ||
            type == typeof(DateTime?) || type == typeof(DateOnly) || type == typeof(TimeSpan) ||
            type == typeof(decimal) || type == typeof(int) || type == typeof(long) || type == typeof(short) ||
            type == typeof(double) || type == typeof(float) || type == typeof(Guid) ||
            Nullable.GetUnderlyingType(type) is { } underlying && IsReportScalar(underlying);

        private static string GetPropertyLabel(PropertyInfo property) =>
            property.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()?.Name ??
            Regex.Replace(property.Name, "([a-z0-9])([A-Z])", "$1 $2");

        private static string FormatValue(object? value) => value switch
        {
            null => string.Empty,
            DateTime date => date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeSpan time => time.ToString("c", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };

        private sealed record ReportDefinition(Type EntityType, PropertyInfo DateProperty, List<PropertyInfo> Properties, string Label);
        private sealed record ReportData(List<string> Columns, List<List<string>> Rows);
    }
}
