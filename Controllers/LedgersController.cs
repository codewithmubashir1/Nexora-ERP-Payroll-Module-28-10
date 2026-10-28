using ERP.Identity.Data;
using ERP.Identity.Models.ViewModels.Accounts;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Accounts/Ledgers")]
    public class LedgersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILedgerService _ledgerService;

        public LedgersController(ApplicationDbContext db, ILedgerService ledgerService)
        {
            _db = db;
            _ledgerService = ledgerService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var vm = new LedgerViewModel
            {
                Customers = await _db.Customers.OrderBy(c => c.Name).Select(c => new CustomerLite(c.Id, c.Name, c.CustomerCode)).ToListAsync(),
                Vendors = await _db.Vendors.OrderBy(v => v.Name).Select(v => new VendorLite(v.Id, v.Name, v.VendorCode)).ToListAsync(),
                Employees = await _db.Employees.OrderBy(e => e.Name).Select(e => new EmployeeOption(e.Id, e.Name, e.EmployeeCode)).ToListAsync()
            };

            return View(vm);
        }

        [HttpGet("GetLedger")]
        public async Task<IActionResult> GetLedger(string partyType, int partyId, DateTime? fromDate, DateTime? toDate)
        {
            var result = partyType switch
            {
                "Customer" => await _ledgerService.BuildCustomerLedgerAsync(partyId, fromDate, toDate),
                "Vendor" => await _ledgerService.BuildVendorLedgerAsync(partyId, fromDate, toDate),
                "Employee" => await _ledgerService.BuildEmployeeLedgerAsync(partyId, fromDate, toDate),
                _ => null
            };

            if (result == null) return BadRequest("Unknown party type.");

            return Json(new
            {
                openingBalance = result.OpeningBalance,
                closingBalance = result.ClosingBalance,
                totalDebit = result.Rows.Sum(r => r.Debit),
                totalCredit = result.Rows.Sum(r => r.Credit),
                rows = result.Rows
            });
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export(string partyType, int partyId, DateTime? fromDate, DateTime? toDate)
        {
            var result = partyType switch
            {
                "Customer" => await _ledgerService.BuildCustomerLedgerAsync(partyId, fromDate, toDate),
                "Vendor" => await _ledgerService.BuildVendorLedgerAsync(partyId, fromDate, toDate),
                "Employee" => await _ledgerService.BuildEmployeeLedgerAsync(partyId, fromDate, toDate),
                _ => null
            };
            if (result == null) return BadRequest("Unknown party type.");

            var headers = new[] { "Date", "Document Type", "Document Number", "Description", "Debit", "Credit", "Running Balance" };
            var rows = result.Rows.Select(row => new string?[]
            {
                row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), row.DocumentType, row.DocumentNumber,
                row.Description, row.Debit.ToString(CultureInfo.InvariantCulture), row.Credit.ToString(CultureInfo.InvariantCulture),
                row.RunningBalance.ToString(CultureInfo.InvariantCulture)
            });
            var filename = $"{partyType.ToLowerInvariant()}_ledger_{partyId}_{DateTime.Today:yyyyMMdd}.csv";
            return File(CsvUtility.WriteWithHeader(headers, rows), "text/csv", filename);
        }
    }
}
