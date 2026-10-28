using ERP.Identity.Data;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Accounts;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Accounts/AccountStatements")]
    public class AccountStatementsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILedgerService _ledgerService;
        private readonly ICompanyInfoService _companyInfoService;

        public AccountStatementsController(ApplicationDbContext db, ILedgerService ledgerService, ICompanyInfoService companyInfoService)
        {
            _db = db;
            _ledgerService = ledgerService;
            _companyInfoService = companyInfoService;
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

        [HttpGet("Print")]
        public async Task<IActionResult> Print(string partyType, int partyId, DateTime? fromDate, DateTime? toDate)
        {
            string partyName;
            string? partyCode = null, partyAddress = null, partyEmail = null, partyPhone = null, partyNTN = null, partySTRN = null;

            switch (partyType)
            {
                case "Customer":
                    var customer = await _db.Customers.FindAsync(partyId);
                    if (customer == null) return NotFound();
                    partyName = customer.Name; partyCode = customer.CustomerCode; partyAddress = customer.Address;
                    partyEmail = customer.Email; partyPhone = customer.Phone; partyNTN = customer.NTNNumber; partySTRN = customer.SalesTaxRegistrationNumber;
                    break;
                case "Vendor":
                    var vendor = await _db.Vendors.FindAsync(partyId);
                    if (vendor == null) return NotFound();
                    partyName = vendor.Name; partyCode = vendor.VendorCode; partyAddress = vendor.Address;
                    partyEmail = vendor.Email; partyPhone = vendor.Phone; partyNTN = vendor.NTNNumber; partySTRN = vendor.SalesTaxRegistrationNumber;
                    break;
                case "Employee":
                    var employee = await _db.Employees.FindAsync(partyId);
                    if (employee == null) return NotFound();
                    partyName = employee.Name; partyCode = employee.EmployeeCode; partyEmail = employee.Email; partyPhone = employee.Phone;
                    break;
                default:
                    return BadRequest("Unknown party type.");
            }

            var result = partyType switch
            {
                "Customer" => await _ledgerService.BuildCustomerLedgerAsync(partyId, fromDate, toDate),
                "Vendor" => await _ledgerService.BuildVendorLedgerAsync(partyId, fromDate, toDate),
                _ => await _ledgerService.BuildEmployeeLedgerAsync(partyId, fromDate, toDate)
            };

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "ACCOUNT STATEMENT",
                DocumentNumberLabel = "STATEMENT PERIOD",
                DocumentNumber = fromDate.HasValue && toDate.HasValue
                    ? $"{fromDate.Value:dd-MM-yyyy} to {toDate.Value:dd-MM-yyyy}"
                    : "All Time",
                ExtraRefs = new() { ("GENERATED ON", DateTime.UtcNow.ToString("dd-MM-yyyy")) },
                PartyLabel = partyType.ToUpper(),
                PartyCode = partyCode,
                PartyName = partyName,
                PartyAddress = partyAddress,
                PartyEmail = partyEmail,
                PartyPhone = partyPhone,
                PartyNTN = partyNTN,
                PartySTRN = partySTRN,
                Columns = new() { "Date", "Document Type", "Document #", "Description", "Debit (Rs.)", "Credit (Rs.)", "Balance (Rs.)" },
                Rows = result.Rows.Select(r => new List<string>
                {
                    r.Date.ToString("dd-MM-yyyy"), r.DocumentType, r.DocumentNumber, r.Description ?? "—",
                    r.Debit > 0 ? r.Debit.ToString("N2") : "—", r.Credit > 0 ? r.Credit.ToString("N2") : "—", r.RunningBalance.ToString("N2")
                }).ToList(),
                Totals = new()
                {
                    ("Opening Balance", result.OpeningBalance.ToString("N2"), false),
                    ("Total Debit", result.Rows.Sum(r => r.Debit).ToString("N2"), false),
                    ("Total Credit", result.Rows.Sum(r => r.Credit).ToString("N2"), false),
                    ("Closing Balance", result.ClosingBalance.ToString("N2"), true)
                },
                AmountInWords = NumberToWordsConverter.ToWords(Math.Abs(result.ClosingBalance)),
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }
    }
}
