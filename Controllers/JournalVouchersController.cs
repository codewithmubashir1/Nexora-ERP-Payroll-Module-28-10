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
    [Route("Accounts/JournalVouchers")]
    public class JournalVouchersController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IAccountingService _accountingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public JournalVouchersController(ApplicationDbContext db, IAccountingService accountingService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.JournalVouchers
                .Include(j => j.Lines)
                .OrderByDescending(j => j.Id)
                .Select(j => new JournalVoucherListItemViewModel
                {
                    Id = j.Id,
                    VoucherNumber = j.VoucherNumber,
                    VoucherDate = j.VoucherDate,
                    Narration = j.Narration,
                    TotalAmount = j.TotalAmount,
                    LineCount = j.Lines.Count,
                    Status = j.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new JournalVoucherViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var jv = await _db.JournalVouchers
                .Include(j => j.Lines).ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jv == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Accountant);
            ViewBag.PartyNames = await BuildPartyNameLookupAsync(jv.Lines.Select(l => (l.PartyType, l.PartyId)));
            return View(jv);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var jv = await _db.JournalVouchers
                .Include(j => j.Lines).ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jv == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "JOURNAL VOUCHER",
                DocumentNumberLabel = "JV #",
                DocumentNumber = jv.VoucherNumber,
                Status = jv.Status.ToString(),
                ExtraRefs = new() { ("DATE", jv.VoucherDate.ToString("dd-MM-yyyy")) },
                PartyLabel = "NARRATION",
                PartyName = jv.Narration ?? "—",
                Columns = new() { "Account Code", "Account Name", "Debit (Rs.)", "Credit (Rs.)", "Line Narration" },
                Rows = jv.Lines.Select(l => new List<string>
                {
                    l.Account?.AccountCode ?? "", l.Account?.AccountName ?? "",
                    l.Debit > 0 ? l.Debit.ToString("N2") : "—", l.Credit > 0 ? l.Credit.ToString("N2") : "—",
                    l.Narration ?? "—"
                }).ToList(),
                Totals = new()
                {
                    ("Total Debit", jv.Lines.Sum(l => l.Debit).ToString("N2"), false),
                    ("Total Credit", jv.Lines.Sum(l => l.Credit).ToString("N2"), true)
                },
                AmountInWords = NumberToWordsConverter.ToWords(jv.TotalAmount),
                ApprovedByName = jv.ApprovedByName,
                ApprovedOn = jv.ApprovedOn,
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StandardUser}")]
        public async Task<IActionResult> Save(JournalVoucherViewModel model)
        {
            model.Lines = (model.Lines ?? new()).Where(l => l.AccountId != 0 && (l.Debit > 0 || l.Credit > 0)).ToList();

            if (model.Lines.Count < 2)
                ModelState.AddModelError(string.Empty, "A journal voucher needs at least two lines.");

            var totalDebit = model.Lines.Sum(l => l.Debit);
            var totalCredit = model.Lines.Sum(l => l.Credit);
            if (Math.Round(totalDebit, 2) != Math.Round(totalCredit, 2))
                ModelState.AddModelError(string.Empty, $"Debits ({totalDebit:N2}) must equal credits ({totalCredit:N2}) before this voucher can be saved.");

            if (model.Lines.Any(l => l.Debit > 0 && l.Credit > 0))
                ModelState.AddModelError(string.Empty, "A single line cannot have both a debit and a credit — use two lines instead.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var jv = new JournalVoucher
            {
                VoucherDate = model.VoucherDate,
                Narration = model.Narration,
                TotalAmount = totalDebit,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };
            jv.VoucherNumber = await GenerateVoucherNumberAsync();

            foreach (var line in model.Lines)
            {
                jv.Lines.Add(new JournalVoucherLine
                {
                    AccountId = line.AccountId,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    PartyType = line.PartyType,
                    PartyId = line.PartyId,
                    Narration = line.Narration
                });
            }

            _db.JournalVouchers.Add(jv);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Journal voucher {jv.VoucherNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var jv = await _db.JournalVouchers.Include(j => j.Lines).FirstOrDefaultAsync(j => j.Id == id);
            if (jv == null) return Json(new { success = false, message = "Journal voucher not found." });
            if (jv.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending journal vouchers can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            jv.Status = VoucherStatus.Approved;
            jv.ApprovedById = currentUser?.Id;
            jv.ApprovedByName = currentUser?.FullName;
            jv.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var glLines = jv.Lines.Select(l => new JournalLineInput(l.AccountId, l.Debit, l.Credit, l.PartyType, l.PartyId, l.Narration));
            await _accountingService.PostJournalEntryAsync("JournalVoucher", jv.Id, jv.VoucherDate, jv.Narration, glLines);

            return Json(new { success = true, message = $"Journal voucher {jv.VoucherNumber} approved and posted to the ledger." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var jv = await _db.JournalVouchers.FindAsync(request.Id);
            if (jv == null) return Json(new { success = false, message = "Journal voucher not found." });
            if (jv.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending journal vouchers can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            jv.Status = VoucherStatus.Rejected;
            jv.ApprovedById = currentUser?.Id;
            jv.ApprovedByName = currentUser?.FullName;
            jv.ApprovedOn = DateTime.UtcNow;
            jv.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Journal voucher {jv.VoucherNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var jv = await _db.JournalVouchers.Include(j => j.Lines).FirstOrDefaultAsync(j => j.Id == id);
            if (jv == null) return Json(new { success = false, message = "Journal voucher not found." });

            if (jv.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) journal vouchers can be deleted." });

            _db.JournalVoucherLines.RemoveRange(jv.Lines);
            _db.JournalVouchers.Remove(jv);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Journal voucher deleted." });
        }

        private async Task<string> GenerateVoucherNumberAsync()
        {
            var monthStamp = DateTime.UtcNow.ToString("yyyyMM");
            var fullPrefix = $"JV-{monthStamp}-";
            var count = await _db.JournalVouchers.CountAsync(j => j.VoucherNumber.StartsWith(fullPrefix));
            return $"{fullPrefix}{(count + 1):D4}";
        }

        private async Task<Dictionary<string, string>> BuildPartyNameLookupAsync(IEnumerable<(PartyType PartyType, int? PartyId)> parties)
        {
            var result = new Dictionary<string, string>();
            var customerIds = parties.Where(p => p.PartyType == PartyType.Customer && p.PartyId.HasValue).Select(p => p.PartyId!.Value).Distinct().ToList();
            var vendorIds = parties.Where(p => p.PartyType == PartyType.Vendor && p.PartyId.HasValue).Select(p => p.PartyId!.Value).Distinct().ToList();
            var employeeIds = parties.Where(p => p.PartyType == PartyType.Employee && p.PartyId.HasValue).Select(p => p.PartyId!.Value).Distinct().ToList();

            if (customerIds.Any())
                foreach (var c in await _db.Customers.Where(c => customerIds.Contains(c.Id)).ToListAsync())
                    result[$"Customer:{c.Id}"] = c.Name;

            if (vendorIds.Any())
                foreach (var v in await _db.Vendors.Where(v => vendorIds.Contains(v.Id)).ToListAsync())
                    result[$"Vendor:{v.Id}"] = v.Name;

            if (employeeIds.Any())
                foreach (var e in await _db.Employees.Where(e => employeeIds.Contains(e.Id)).ToListAsync())
                    result[$"Employee:{e.Id}"] = e.Name;

            return result;
        }

        private async Task PopulateDropdownsAsync(JournalVoucherViewModel vm)
        {
            vm.Accounts = await _db.Accounts.Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .Select(a => new AccountOption(a.Id, a.AccountCode, a.AccountName, a.AccountType))
                .ToListAsync();

            vm.Customers = await _db.Customers.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CustomerLite(c.Id, c.Name, c.CustomerCode))
                .ToListAsync();

            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new VendorLite(v.Id, v.Name, v.VendorCode))
                .ToListAsync();

            vm.Employees = await _db.Employees.Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new EmployeeOption(e.Id, e.Name, e.EmployeeCode))
                .ToListAsync();

            if (vm.Lines.Count < 2) vm.Lines = new() { new(), new() };
        }
    }
}
