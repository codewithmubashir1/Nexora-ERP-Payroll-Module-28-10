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
    [Route("Purchase/Cheques")]
    public class IssuedChequesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public IssuedChequesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.IssuedCheques
                .Include(c => c.Vendor)
                .OrderByDescending(c => c.Id)
                .Select(c => new IssuedChequeListItemViewModel
                {
                    Id = c.Id,
                    ChequeReferenceNo = c.ChequeReferenceNo,
                    ChequeNumber = c.ChequeNumber,
                    BankName = c.BankName,
                    ChequeDate = c.ChequeDate,
                    VendorName = c.Vendor!.Name,
                    Amount = c.Amount,
                    ClearanceStatus = c.ClearanceStatus.ToString(),
                    Status = c.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var cheque = await _db.IssuedCheques.Include(c => c.Vendor).FirstOrDefaultAsync(c => c.Id == id);
            if (cheque == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "ISSUED CHEQUE",
                DocumentNumberLabel = "REFERENCE #",
                DocumentNumber = cheque.ChequeReferenceNo,
                Status = cheque.Status.ToString(),
                ExtraRefs = new()
                {
                    ("CHEQUE #", cheque.ChequeNumber),
                    ("BANK", cheque.BankName),
                    ("DATE", cheque.ChequeDate.ToString("dd-MM-yyyy")),
                    ("CLEARANCE STATUS", cheque.ClearanceStatus.ToString())
                },
                PartyLabel = "PAYEE (VENDOR)",
                PartyCode = cheque.Vendor?.VendorCode,
                PartyName = cheque.Vendor?.Name,
                PartyAddress = cheque.Vendor?.Address,
                PartyEmail = cheque.Vendor?.Email,
                PartyPhone = cheque.Vendor?.Phone,
                PartyNTN = cheque.Vendor?.NTNNumber,
                PartySTRN = cheque.Vendor?.SalesTaxRegistrationNumber,
                Columns = new() { "Description", "Amount (Rs.)" },
                Rows = new() { new() { $"Cheque issued to {cheque.Vendor?.Name}", cheque.Amount.ToString("N2") } },
                Totals = new() { ("Amount", cheque.Amount.ToString("N2"), true) },
                AmountInWords = NumberToWordsConverter.ToWords(cheque.Amount),
                AdditionalNotes = cheque.Remarks,
                ApprovedByName = cheque.ApprovedByName,
                ApprovedOn = cheque.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new IssuedChequeViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(IssuedChequeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var cheque = new IssuedCheque
            {
                ChequeReferenceNo = await _stockService.GenerateDocumentNumberAsync("CHQ", _db.IssuedCheques.Select(c => c.ChequeReferenceNo)),
                ChequeNumber = model.ChequeNumber,
                BankName = model.BankName,
                ChequeDate = model.ChequeDate,
                VendorId = model.VendorId,
                Amount = model.Amount,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            _db.IssuedCheques.Add(cheque);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cheque {cheque.ChequeReferenceNo} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var cheque = await _db.IssuedCheques.FindAsync(id);
            if (cheque == null) return Json(new { success = false, message = "Cheque not found." });
            if (cheque.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending cheques can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            cheque.Status = VoucherStatus.Approved;
            cheque.ApprovedById = currentUser?.Id;
            cheque.ApprovedByName = currentUser?.FullName;
            cheque.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Cheque {cheque.ChequeReferenceNo} approved and marked as issued." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var cheque = await _db.IssuedCheques.FindAsync(request.Id);
            if (cheque == null) return Json(new { success = false, message = "Cheque not found." });
            if (cheque.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending cheques can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            cheque.Status = VoucherStatus.Rejected;
            cheque.ClearanceStatus = ChequeClearanceStatus.Cancelled;
            cheque.ApprovedById = currentUser?.Id;
            cheque.ApprovedByName = currentUser?.FullName;
            cheque.ApprovedOn = DateTime.UtcNow;
            cheque.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Cheque {cheque.ChequeReferenceNo} rejected." });
        }

        /// <summary>Marks an already-issued (Approved) cheque as Cleared or Bounced at the bank.</summary>
        [HttpPost("SetClearance")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> SetClearance(int id, ChequeClearanceStatus status)
        {
            var cheque = await _db.IssuedCheques.FindAsync(id);
            if (cheque == null) return Json(new { success = false, message = "Cheque not found." });
            if (cheque.Status != VoucherStatus.Approved) return Json(new { success = false, message = "Only issued (approved) cheques can have their clearance status updated." });

            cheque.ClearanceStatus = status;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Cheque {cheque.ChequeReferenceNo} marked as {status}." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var cheque = await _db.IssuedCheques.FindAsync(id);
            if (cheque == null) return Json(new { success = false, message = "Cheque not found." });

            if (cheque.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) cheques can be deleted." });

            var isReferenced =
                await _db.PurchaseInvoicePayments.AnyAsync(p => p.IssuedChequeId == id) ||
                await _db.VendorAdvances.AnyAsync(a => a.IssuedChequeId == id);
            if (isReferenced)
                return Json(new { success = false, message = "This cheque is referenced by a payment or advance and cannot be deleted." });

            _db.IssuedCheques.Remove(cheque);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Cheque deleted." });
        }

        private async Task PopulateDropdownsAsync(IssuedChequeViewModel vm)
        {
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new VendorOption(v.Id, v.Name, v.VendorCode))
                .ToListAsync();
        }
    }
}
