using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Purchase;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Accounts;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Accounts/BankPaymentVouchers")]
    public class BankPaymentVouchersController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IAccountingService _accountingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public BankPaymentVouchersController(ApplicationDbContext db, IAccountingService accountingService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
        {
            _db = db;
            _accountingService = accountingService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.BankPaymentVouchers
                .Include(v => v.ContraAccount)
                .OrderByDescending(v => v.Id)
                .Select(v => new SimpleVoucherListItemViewModel
                {
                    Id = v.Id,
                    VoucherNumber = v.VoucherNumber,
                    VoucherDate = v.VoucherDate,
                    ContraAccountName = v.ContraAccount!.AccountName,
                    Amount = v.Amount,
                    PartyOrNote = v.PaidTo,
                    Status = v.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new BankPaymentVoucherViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var v = await _db.BankPaymentVouchers.Include(x => x.BankAccount).Include(x => x.ContraAccount).FirstOrDefaultAsync(x => x.Id == id);
            if (v == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "BANK PAYMENT VOUCHER",
                DocumentNumberLabel = "BPV #",
                DocumentNumber = v.VoucherNumber,
                Status = v.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", v.VoucherDate.ToString("dd-MM-yyyy")),
                    ("BANK ACCOUNT", v.BankAccount?.AccountName ?? "—"),
                    ("DEBIT ACCOUNT", v.ContraAccount?.AccountName ?? "—"),
                    ("METHOD", v.PaymentMethod.ToString()),
                    ("CHEQUE #", v.ChequeNumber ?? "—")
                },
                PartyLabel = "PAID TO",
                PartyName = v.PaidTo ?? "—",
                Columns = new() { "Description", "Amount (Rs.)" },
                Rows = new() { new() { v.Narration ?? $"Bank payment — {v.ContraAccount?.AccountName}", v.Amount.ToString("N2") } },
                Totals = new() { ("Amount Paid", v.Amount.ToString("N2"), true) },
                AmountInWords = NumberToWordsConverter.ToWords(v.Amount),
                ApprovedByName = v.ApprovedByName,
                ApprovedOn = v.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StandardUser}")]
        public async Task<IActionResult> Save(BankPaymentVoucherViewModel model)
        {
            if (model.PaymentMethod == PaymentMethod.Cheque && string.IsNullOrWhiteSpace(model.ChequeNumber))
                ModelState.AddModelError(nameof(model.ChequeNumber), "Enter the cheque number for this payment.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var v = new BankPaymentVoucher
            {
                VoucherDate = model.VoucherDate,
                BankAccountId = model.BankAccountId,
                ContraAccountId = model.ContraAccountId,
                Amount = model.Amount,
                PaidTo = model.PaidTo,
                PartyType = model.PartyType,
                PartyId = model.PartyType == PartyType.None ? null : model.PartyId,
                PaymentMethod = model.PaymentMethod,
                ChequeNumber = model.PaymentMethod == PaymentMethod.Cheque ? model.ChequeNumber : null,
                ChequeDate = model.PaymentMethod == PaymentMethod.Cheque ? model.ChequeDate : null,
                ClearanceStatus = model.PaymentMethod == PaymentMethod.Cheque ? ChequeClearanceStatus.Outstanding : null,
                Narration = model.Narration,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };
            v.VoucherNumber = await GenerateVoucherNumberAsync();

            _db.BankPaymentVouchers.Add(v);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Bank payment voucher {v.VoucherNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var v = await _db.BankPaymentVouchers.FirstOrDefaultAsync(x => x.Id == id);
            if (v == null) return Json(new { success = false, message = "Voucher not found." });
            if (v.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending vouchers can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            v.Status = VoucherStatus.Approved;
            v.ApprovedById = currentUser?.Id;
            v.ApprovedByName = currentUser?.FullName;
            v.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _accountingService.PostJournalEntryAsync("BankPaymentVoucher", v.Id, v.VoucherDate, v.Narration ?? v.PaidTo, new[]
            {
                new JournalLineInput(v.ContraAccountId, v.Amount, 0, v.PartyType, v.PartyId, v.PaidTo),
                new JournalLineInput(v.BankAccountId, 0, v.Amount)
            });

            return Json(new { success = true, message = $"Voucher {v.VoucherNumber} approved and posted to the ledger." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var v = await _db.BankPaymentVouchers.FindAsync(request.Id);
            if (v == null) return Json(new { success = false, message = "Voucher not found." });
            if (v.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending vouchers can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            v.Status = VoucherStatus.Rejected;
            v.ApprovedById = currentUser?.Id;
            v.ApprovedByName = currentUser?.FullName;
            v.ApprovedOn = DateTime.UtcNow;
            v.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Voucher {v.VoucherNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var v = await _db.BankPaymentVouchers.FindAsync(id);
            if (v == null) return Json(new { success = false, message = "Voucher not found." });
            if (v.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending (not yet approved) vouchers can be deleted." });

            _db.BankPaymentVouchers.Remove(v);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Voucher deleted." });
        }

        private async Task<string> GenerateVoucherNumberAsync()
        {
            var monthStamp = DateTime.UtcNow.ToString("yyyyMM");
            var fullPrefix = $"BPV-{monthStamp}-";
            var count = await _db.BankPaymentVouchers.CountAsync(v => v.VoucherNumber.StartsWith(fullPrefix));
            return $"{fullPrefix}{(count + 1):D4}";
        }

        private async Task PopulateDropdownsAsync(BankPaymentVoucherViewModel vm)
        {
            var accounts = await _db.Accounts.Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .Select(a => new AccountOption(a.Id, a.AccountCode, a.AccountName, a.AccountType))
                .ToListAsync();

            vm.BankAccounts = accounts.Where(a => a.AccountType == AccountType.Asset).ToList();
            vm.ContraAccounts = accounts;

            vm.Customers = await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).Select(c => new CustomerLite(c.Id, c.Name, c.CustomerCode)).ToListAsync();
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive).OrderBy(v => v.Name).Select(v => new VendorLite(v.Id, v.Name, v.VendorCode)).ToListAsync();
            vm.Employees = await _db.Employees.Where(e => e.IsActive).OrderBy(e => e.Name).Select(e => new EmployeeOption(e.Id, e.Name, e.EmployeeCode)).ToListAsync();
        }
    }
}
