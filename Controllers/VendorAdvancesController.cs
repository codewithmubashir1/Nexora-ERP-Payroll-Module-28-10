using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Purchase;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Models.ViewModels.Purchase;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Purchase/Advances")]
    public class VendorAdvancesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public VendorAdvancesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
        {
            _db = db;
            _stockService = stockService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var advances = await _db.VendorAdvances.Include(a => a.Vendor).OrderByDescending(a => a.Id).ToListAsync();

            var list = advances.Select(a => new VendorAdvanceListItemViewModel
            {
                Id = a.Id,
                AdvanceNumber = a.AdvanceNumber,
                AdvanceDate = a.AdvanceDate,
                VendorName = a.Vendor!.Name,
                Amount = a.Amount,
                AdjustedAmount = a.AdjustedAmount,
                AvailableAmount = a.AvailableAmount,
                Status = a.Status.ToString()
            }).ToList();

            return Json(new { data = list });
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var advance = await _db.VendorAdvances.Include(a => a.Vendor).Include(a => a.IssuedCheque).FirstOrDefaultAsync(a => a.Id == id);
            if (advance == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "VENDOR ADVANCE",
                DocumentNumberLabel = "ADVANCE #",
                DocumentNumber = advance.AdvanceNumber,
                Status = advance.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", advance.AdvanceDate.ToString("dd-MM-yyyy")),
                    ("METHOD", advance.PaymentMethod.ToString()),
                    ("CHEQUE #", advance.IssuedCheque?.ChequeReferenceNo ?? "—"),
                    ("ADJUSTED", advance.AdjustedAmount.ToString("N2")),
                    ("AVAILABLE", advance.AvailableAmount.ToString("N2"))
                },
                PartyLabel = "VENDOR",
                PartyCode = advance.Vendor?.VendorCode,
                PartyName = advance.Vendor?.Name,
                PartyAddress = advance.Vendor?.Address,
                PartyEmail = advance.Vendor?.Email,
                PartyPhone = advance.Vendor?.Phone,
                PartyNTN = advance.Vendor?.NTNNumber,
                PartySTRN = advance.Vendor?.SalesTaxRegistrationNumber,
                Columns = new() { "Description", "Amount (Rs.)" },
                Rows = new() { new() { $"Advance paid to {advance.Vendor?.Name}", advance.Amount.ToString("N2") } },
                Totals = new() { ("Advance Amount", advance.Amount.ToString("N2"), true) },
                AmountInWords = NumberToWordsConverter.ToWords(advance.Amount),
                AdditionalNotes = advance.Remarks,
                ApprovedByName = advance.ApprovedByName,
                ApprovedOn = advance.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new VendorAdvanceViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(VendorAdvanceViewModel model)
        {
            if (model.PaymentMethod == Models.Purchase.PaymentMethod.Cheque && model.IssuedChequeId == null)
                ModelState.AddModelError(nameof(model.IssuedChequeId), "Select the cheque issued for this advance.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var advance = new VendorAdvance
            {
                AdvanceNumber = await _stockService.GenerateDocumentNumberAsync("ADV", _db.VendorAdvances.Select(a => a.AdvanceNumber)),
                AdvanceDate = model.AdvanceDate,
                VendorId = model.VendorId,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                IssuedChequeId = model.PaymentMethod == Models.Purchase.PaymentMethod.Cheque ? model.IssuedChequeId : null,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            _db.VendorAdvances.Add(advance);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Advance {advance.AdvanceNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var advance = await _db.VendorAdvances.FindAsync(id);
            if (advance == null) return Json(new { success = false, message = "Advance not found." });
            if (advance.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending advances can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            advance.Status = VoucherStatus.Approved;
            advance.ApprovedById = currentUser?.Id;
            advance.ApprovedByName = currentUser?.FullName;
            advance.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Advance {advance.AdvanceNumber} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var advance = await _db.VendorAdvances.FindAsync(request.Id);
            if (advance == null) return Json(new { success = false, message = "Advance not found." });
            if (advance.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending advances can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            advance.Status = VoucherStatus.Rejected;
            advance.ApprovedById = currentUser?.Id;
            advance.ApprovedByName = currentUser?.FullName;
            advance.ApprovedOn = DateTime.UtcNow;
            advance.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Advance {advance.AdvanceNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var advance = await _db.VendorAdvances.FindAsync(id);
            if (advance == null) return Json(new { success = false, message = "Advance not found." });

            if (advance.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) advances can be deleted." });

            _db.VendorAdvances.Remove(advance);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Advance deleted." });
        }

        private async Task PopulateDropdownsAsync(VendorAdvanceViewModel vm)
        {
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new VendorOption(v.Id, v.Name, v.VendorCode))
                .ToListAsync();

            vm.Cheques = await _db.IssuedCheques.Where(c => c.Status == VoucherStatus.Approved)
                .OrderByDescending(c => c.Id)
                .Select(c => new ChequeOption(c.Id, c.ChequeReferenceNo, c.ChequeNumber))
                .ToListAsync();
        }
    }
}
