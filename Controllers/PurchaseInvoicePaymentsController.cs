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
    [Route("Purchase/Payments")]
    public class PurchaseInvoicePaymentsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public PurchaseInvoicePaymentsController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.PurchaseInvoicePayments
                .Include(p => p.Vendor)
                .Include(p => p.PurchaseInvoice)
                .OrderByDescending(p => p.Id)
                .Select(p => new PurchaseInvoicePaymentListItemViewModel
                {
                    Id = p.Id,
                    PaymentNumber = p.PaymentNumber,
                    PaymentDate = p.PaymentDate,
                    VendorName = p.Vendor!.Name,
                    InvoiceNumber = p.PurchaseInvoice!.InvoiceNumber,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var payment = await _db.PurchaseInvoicePayments
                .Include(p => p.Vendor)
                .Include(p => p.PurchaseInvoice)
                .Include(p => p.IssuedCheque)
                .Include(p => p.VendorAdvance)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "PURCHASE INVOICE PAYMENT",
                DocumentNumberLabel = "PAYMENT #",
                DocumentNumber = payment.PaymentNumber,
                Status = payment.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", payment.PaymentDate.ToString("dd-MM-yyyy")),
                    ("INVOICE #", payment.PurchaseInvoice?.InvoiceNumber ?? "—"),
                    ("METHOD", payment.PaymentMethod.ToString()),
                    ("CHEQUE #", payment.IssuedCheque?.ChequeReferenceNo ?? "—"),
                    ("ADVANCE #", payment.VendorAdvance?.AdvanceNumber ?? "—")
                },
                PartyLabel = "VENDOR",
                PartyCode = payment.Vendor?.VendorCode,
                PartyName = payment.Vendor?.Name,
                PartyAddress = payment.Vendor?.Address,
                PartyEmail = payment.Vendor?.Email,
                PartyPhone = payment.Vendor?.Phone,
                PartyNTN = payment.Vendor?.NTNNumber,
                PartySTRN = payment.Vendor?.SalesTaxRegistrationNumber,
                Columns = new() { "Description", "Amount (Rs.)" },
                Rows = new() { new() { $"Payment against invoice {payment.PurchaseInvoice?.InvoiceNumber}", payment.Amount.ToString("N2") } },
                Totals = payment.WithholdingTaxRate.HasValue && payment.WithholdingTaxAmount > 0
                    ? new()
                    {
                        ("Amount", payment.Amount.ToString("N2"), false),
                        ($"Withholding Tax ({payment.WithholdingTaxRate.Value:0.##}%)", $"- {payment.WithholdingTaxAmount:N2}", false),
                        ("Net Payment to Vendor", payment.NetPaymentAmount.ToString("N2"), true)
                    }
                    : new() { ("Amount Paid", payment.Amount.ToString("N2"), true) },
                AmountInWords = NumberToWordsConverter.ToWords(payment.Amount),
                AdditionalNotes = payment.Remarks,
                ApprovedByName = payment.ApprovedByName,
                ApprovedOn = payment.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new PurchaseInvoicePaymentViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(PurchaseInvoicePaymentViewModel model)
        {
            if (model.PaymentMethod == PaymentMethod.Cheque && model.IssuedChequeId == null)
                ModelState.AddModelError(nameof(model.IssuedChequeId), "Select the cheque used for this payment.");

            if (model.VendorAdvanceId.HasValue)
            {
                var advance = await _db.VendorAdvances.FindAsync(model.VendorAdvanceId.Value);
                if (advance == null || advance.AvailableAmount < model.Amount)
                    ModelState.AddModelError(nameof(model.VendorAdvanceId), "The selected advance does not have enough available balance for this amount.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var withholdingTaxAmount = model.WithholdingTaxRate.HasValue
                ? Math.Round(model.Amount * model.WithholdingTaxRate.Value / 100m, 2)
                : 0m;

            var payment = new PurchaseInvoicePayment
            {
                PaymentNumber = await _stockService.GenerateDocumentNumberAsync("PPAY", _db.PurchaseInvoicePayments.Select(p => p.PaymentNumber)),
                PaymentDate = model.PaymentDate,
                VendorId = model.VendorId,
                PurchaseInvoiceId = model.PurchaseInvoiceId,
                Amount = model.Amount,
                WithholdingTaxRate = model.WithholdingTaxRate,
                WithholdingTaxAmount = withholdingTaxAmount,
                NetPaymentAmount = model.Amount - withholdingTaxAmount,
                PaymentMethod = model.PaymentMethod,
                IssuedChequeId = model.PaymentMethod == PaymentMethod.Cheque ? model.IssuedChequeId : null,
                VendorAdvanceId = model.VendorAdvanceId,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            _db.PurchaseInvoicePayments.Add(payment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payment {payment.PaymentNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var payment = await _db.PurchaseInvoicePayments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return Json(new { success = false, message = "Payment not found." });
            if (payment.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending payments can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            payment.Status = VoucherStatus.Approved;
            payment.ApprovedById = currentUser?.Id;
            payment.ApprovedByName = currentUser?.FullName;
            payment.ApprovedOn = DateTime.UtcNow;

            // If this payment draws on a vendor advance, consume that balance now.
            if (payment.VendorAdvanceId.HasValue)
            {
                var advance = await _db.VendorAdvances.FindAsync(payment.VendorAdvanceId.Value);
                if (advance != null) advance.AdjustedAmount += payment.Amount;
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Payment {payment.PaymentNumber} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var payment = await _db.PurchaseInvoicePayments.FindAsync(request.Id);
            if (payment == null) return Json(new { success = false, message = "Payment not found." });
            if (payment.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending payments can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            payment.Status = VoucherStatus.Rejected;
            payment.ApprovedById = currentUser?.Id;
            payment.ApprovedByName = currentUser?.FullName;
            payment.ApprovedOn = DateTime.UtcNow;
            payment.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Payment {payment.PaymentNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _db.PurchaseInvoicePayments.FindAsync(id);
            if (payment == null) return Json(new { success = false, message = "Payment not found." });

            if (payment.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) payments can be deleted." });

            _db.PurchaseInvoicePayments.Remove(payment);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Payment deleted." });
        }

        /// <summary>Feeds the invoice dropdown with each invoice's outstanding balance for quick selection.</summary>
        [HttpGet("GetInvoicesForVendor/{vendorId:int}")]
        public async Task<IActionResult> GetInvoicesForVendor(int vendorId)
        {
            var invoices = await _db.PurchaseInvoices
                .Where(i => i.VendorId == vendorId && i.Status == VoucherStatus.Approved)
                .ToListAsync();

            var paidByInvoice = await _db.PurchaseInvoicePayments
                .Where(p => p.Status == VoucherStatus.Approved && p.PurchaseInvoice!.VendorId == vendorId)
                .GroupBy(p => p.PurchaseInvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            var result = invoices.Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.VendorId,
                Balance = i.NetPayableAmount - (paidByInvoice.TryGetValue(i.Id, out var paid) ? paid : 0)
            }).Where(i => i.Balance > 0);

            return Json(result);
        }

        private async Task PopulateDropdownsAsync(PurchaseInvoicePaymentViewModel vm)
        {
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new VendorOption(v.Id, v.Name, v.VendorCode))
                .ToListAsync();

            var approvedInvoices = await _db.PurchaseInvoices.Where(i => i.Status == VoucherStatus.Approved).ToListAsync();
            var paidByInvoice = await _db.PurchaseInvoicePayments
                .Where(p => p.Status == VoucherStatus.Approved)
                .GroupBy(p => p.PurchaseInvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            vm.Invoices = approvedInvoices
                .Select(i => new InvoiceOption(i.Id, i.InvoiceNumber, i.NetPayableAmount - (paidByInvoice.TryGetValue(i.Id, out var paid) ? paid : 0), i.VendorId))
                .Where(i => i.Balance > 0)
                .ToList();

            vm.Cheques = await _db.IssuedCheques.Where(c => c.Status == VoucherStatus.Approved)
                .OrderByDescending(c => c.Id)
                .Select(c => new ChequeOption(c.Id, c.ChequeReferenceNo, c.ChequeNumber))
                .ToListAsync();

            var advances = await _db.VendorAdvances.Where(a => a.Status == VoucherStatus.Approved).ToListAsync();
            vm.Advances = advances
                .Where(a => a.AvailableAmount > 0)
                .Select(a => new AdvanceOption(a.Id, a.AdvanceNumber, a.AvailableAmount))
                .ToList();
        }
    }
}
