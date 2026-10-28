using System.Text;
using ERP.Identity.Data;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Month-at-a-glance attendance grid: one row per employee, one column per day.</summary>
    [Authorize]
    [Route("Payroll/AttendanceSheet")]
    public class AttendanceSheetController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AttendanceSheetController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public async Task<IActionResult> Index(int? month, int? year, int? departmentId)
        {
            var vm = await BuildAsync(month, year, departmentId);
            return View(vm);
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export(int? month, int? year, int? departmentId)
        {
            var vm = await BuildAsync(month, year, departmentId);

            var sb = new StringBuilder();
            sb.Append("Employee Code,Employee");
            for (var day = 1; day <= vm.DaysInMonth; day++) sb.Append(',').Append(day);
            sb.AppendLine(",Present,Absent,Leave,Half Days,Late Count,Late Hours,Attendance Hours,Overtime Hours");

            foreach (var row in vm.Rows)
            {
                sb.Append(Escape(row.EmployeeCode)).Append(',').Append(Escape(row.EmployeeName));
                foreach (var cell in row.Cells) sb.Append(',').Append(cell.Code);
                sb.Append(',').Append(row.PresentDays)
                  .Append(',').Append(row.AbsentDays)
                  .Append(',').Append(row.LeaveDays)
                  .Append(',').Append(row.HalfDays)
                  .Append(',').Append(row.LateCount)
                  .Append(',').Append(row.LateHours.ToString("0.00"))
                  .Append(',').Append(row.AttendanceHours.ToString("0.00"))
                  .Append(',').Append(row.OvertimeHours.ToString("0.00"))
                  .AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"attendance_sheet_{vm.Year}_{vm.Month:D2}.csv");
        }

        private async Task<AttendanceSheetViewModel> BuildAsync(int? month, int? year, int? departmentId)
        {
            var vm = new AttendanceSheetViewModel
            {
                Month = Math.Clamp(month ?? DateTime.Today.Month, 1, 12),
                Year = year ?? DateTime.Today.Year,
                DepartmentId = departmentId
            };

            vm.DaysInMonth = DateTime.DaysInMonth(vm.Year, vm.Month);
            var monthStart = new DateTime(vm.Year, vm.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            vm.Departments = await _db.Departments.Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentOption(d.Id, d.Name, d.Code))
                .ToListAsync();

            vm.Holidays = await _db.Holidays
                .Where(h => h.Date >= monthStart && h.Date <= monthEnd)
                .ToDictionaryAsync(h => h.Date.Day, h => h.Name);

            var query = _db.Employees
                .Include(e => e.DepartmentRef)
                .Include(e => e.ShiftTiming)
                .Where(e => e.IsActive);

            if (departmentId.HasValue) query = query.Where(e => e.DepartmentId == departmentId.Value);

            var employees = await query.OrderBy(e => e.Name).ToListAsync();
            var employeeIds = employees.Select(e => e.Id).ToList();

            var records = await _db.AttendanceRecords
                .Where(a => employeeIds.Contains(a.EmployeeId) && a.Date >= monthStart && a.Date <= monthEnd)
                .ToListAsync();

            foreach (var employee in employees)
            {
                var shiftHours = employee.ShiftTiming?.ShiftHours ?? 8m;
                var breakMinutes = employee.ShiftTiming?.BreakMinutes ?? 0;
                var mine = records.Where(r => r.EmployeeId == employee.Id).ToList();

                var row = new AttendanceSheetRowViewModel
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.Name,
                    DepartmentName = employee.DepartmentRef?.Name ?? employee.Department
                };

                for (var day = 1; day <= vm.DaysInMonth; day++)
                {
                    var record = mine.FirstOrDefault(r => r.Date.Day == day);
                    row.Cells.Add(BuildCell(day, record, vm.Holidays.ContainsKey(day)));
                }

                row.PresentDays = mine.Count(r => r.Status == AttendanceStatus.Present);
                row.AbsentDays = mine.Count(r => r.Status == AttendanceStatus.Absent);
                row.LeaveDays = mine.Count(r => r.Status == AttendanceStatus.Leave);
                row.HalfDays = mine.Count(r => r.Status == AttendanceStatus.HalfDay);
                row.LateCount = mine.Count(r => r.IsLate);
                row.LateHours = Math.Round(mine.Sum(r => r.LateByMinutes) / 60m, 2);
                row.AttendanceHours = Math.Round(mine.Sum(r => AttendanceTimeCalculator.WorkedHours(r, breakMinutes)), 2);
                row.OvertimeHours = Math.Round(mine.Sum(r => AttendanceTimeCalculator.OvertimeHours(r, shiftHours, breakMinutes)), 2);

                vm.Rows.Add(row);
            }

            return vm;
        }

        private static AttendanceSheetCellViewModel BuildCell(int day, AttendanceRecord? record, bool isHoliday)
        {
            if (record == null)
            {
                return new AttendanceSheetCellViewModel
                {
                    Day = day,
                    Code = isHoliday ? "H" : "-",
                    CssClass = isHoliday ? "att-holiday" : "att-none",
                    Tooltip = isHoliday ? "Public holiday" : "No entry"
                };
            }

            var (code, css) = record.Status switch
            {
                AttendanceStatus.Present => ("P", "att-present"),
                AttendanceStatus.Absent => ("A", "att-absent"),
                AttendanceStatus.Leave => ("L", "att-leave"),
                AttendanceStatus.Holiday => ("H", "att-holiday"),
                AttendanceStatus.WeekOff => ("W", "att-weekoff"),
                AttendanceStatus.HalfDay => ("½", "att-halfday"),
                _ => ("-", "att-none")
            };

            var tooltip = record.Status.ToString();
            if (record.CheckInTime.HasValue || record.CheckOutTime.HasValue)
                tooltip += $" · {record.CheckInTime:hh\\:mm} – {record.CheckOutTime:hh\\:mm}";
            if (record.IsLate) tooltip += $" · late {record.LateByMinutes}m";
            if (record.IsEarlyDeparture) tooltip += $" · early {record.EarlyByMinutes}m";

            if (record.IsLate) css += " att-late";

            return new AttendanceSheetCellViewModel { Day = day, Code = code, CssClass = css, Tooltip = tooltip };
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        }
    }
}
