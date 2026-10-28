using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;
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
    [Route("Accounts/InventoryVouchers")]
    public class InventoryVouchersController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IAccountingService _accountingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public InventoryVouchersController(ApplicationDbContext db, IAccountingService accountingService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.InventoryVouchers
                .Include(v => v.ContraAccount)
                .OrderByDescending(v => v.Id)
                .Select(v => new InventoryVoucherListItemViewModel
                {
                    Id = v.Id,
                    VoucherNumber = v.VoucherNumber,
                    VoucherDate = v.VoucherDate,
                    VoucherType = v.VoucherType.ToString(),
                    ContraAccountName = v.ContraAccount!.AccountName,
                    Amount = v.Amount,
                    Status = v.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new InventoryVoucherViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var v = await _db.InventoryVouchers
                .Include(x => x.InventoryAccount)
                .Include(x => x.ContraAccount)
                .Include(x => x.ItemAdjustmentNote)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (v == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "INVENTORY VOUCHER",
                DocumentNumberLabel = "IV #",
                DocumentNumber = v.VoucherNumber,
                Status = v.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", v.VoucherDate.ToString("dd-MM-yyyy")),
                    ("TYPE", v.VoucherType.ToString()),
                    ("INVENTORY ACCOUNT", v.InventoryAccount?.AccountName ?? "—"),
                    ("CONTRA ACCOUNT", v.ContraAccount?.AccountName ?? "—"),
                    ("ADJUSTMENT NOTE #", v.ItemAdjustmentNote?.AdjustmentNo ?? "—")
                },
                PartyLabel = "REASON",
                PartyName = v.Narration ?? "—",
                Columns = new() { "Description", "Amount (Rs.)" },
                Rows = new() { new() { $"{v.VoucherType} — {v.InventoryAccount?.AccountName} vs {v.ContraAccount?.AccountName}", v.Amount.ToString("N2") } },
                Totals = new() { ("Amount", v.Amount.ToString("N2"), true) },
                AmountInWords = NumberToWordsConverter.ToWords(v.Amount),
                ApprovedByName = v.ApprovedByName,
                ApprovedOn = v.ApprovedOn,
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(InventoryVoucherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var v = new InventoryVoucher
            {
                VoucherDate = model.VoucherDate,
                VoucherType = model.VoucherType,
                InventoryAccountId = model.InventoryAccountId,
                ContraAccountId = model.ContraAccountId,
                Amount = model.Amount,
                ItemAdjustmentNoteId = model.ItemAdjustmentNoteId,
                Narration = model.Narration,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };
            v.VoucherNumber = await GenerateVoucherNumberAsync();

            _db.InventoryVouchers.Add(v);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Inventory voucher {v.VoucherNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var v = await _db.InventoryVouchers.FirstOrDefaultAsync(x => x.Id == id);
            if (v == null) return Json(new { success = false, message = "Voucher not found." });
            if (v.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending vouchers can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            v.Status = VoucherStatus.Approved;
            v.ApprovedById = currentUser?.Id;
            v.ApprovedByName = currentUser?.FullName;
            v.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Increase: debit Inventory, credit contra. Decrease (write-off): credit
            // Inventory, debit contra (typically an expense account).
            var lines = v.VoucherType == InventoryVoucherType.Increase
                ? new[]
                {
                    new JournalLineInput(v.InventoryAccountId, v.Amount, 0),
                    new JournalLineInput(v.ContraAccountId, 0, v.Amount)
                }
                : new[]
                {
                    new JournalLineInput(v.ContraAccountId, v.Amount, 0),
                    new JournalLineInput(v.InventoryAccountId, 0, v.Amount)
                };

            await _accountingService.PostJournalEntryAsync("InventoryVoucher", v.Id, v.VoucherDate, v.Narration, lines);

            return Json(new { success = true, message = $"Voucher {v.VoucherNumber} approved and posted to the ledger." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var v = await _db.InventoryVouchers.FindAsync(request.Id);
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
            var v = await _db.InventoryVouchers.FindAsync(id);
            if (v == null) return Json(new { success = false, message = "Voucher not found." });
            if (v.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending (not yet approved) vouchers can be deleted." });

            _db.InventoryVouchers.Remove(v);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Voucher deleted." });
        }

        private async Task<string> GenerateVoucherNumberAsync()
        {
            var monthStamp = DateTime.UtcNow.ToString("yyyyMM");
            var fullPrefix = $"IV-{monthStamp}-";
            var count = await _db.InventoryVouchers.CountAsync(v => v.VoucherNumber.StartsWith(fullPrefix));
            return $"{fullPrefix}{(count + 1):D4}";
        }

        private async Task PopulateDropdownsAsync(InventoryVoucherViewModel vm)
        {
            var accounts = await _db.Accounts.Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .Select(a => new AccountOption(a.Id, a.AccountCode, a.AccountName, a.AccountType))
                .ToListAsync();

            vm.InventoryAccounts = accounts.Where(a => a.AccountType == AccountType.Asset).ToList();
            vm.ContraAccounts = accounts;

            vm.AdjustmentNotes = await _db.ItemAdjustmentNotes
                .OrderByDescending(n => n.Id)
                .Select(n => new ItemAdjustmentNoteOption(n.Id, n.AdjustmentNo))
                .ToListAsync();
        }
    }
}
