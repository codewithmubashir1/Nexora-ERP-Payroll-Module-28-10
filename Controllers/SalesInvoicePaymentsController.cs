using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Purchase;
using ERP.Identity.Models.Sales;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Models.ViewModels.Sales;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Sales/Payments")]
    public class SalesInvoicePaymentsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public SalesInvoicePaymentsController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.SalesInvoicePayments
                .Include(p => p.Customer)
                .Include(p => p.SalesInvoice)
                .OrderByDescending(p => p.Id)
                .Select(p => new SalesInvoicePaymentListItemViewModel
                {
                    Id = p.Id,
                    PaymentNumber = p.PaymentNumber,
                    PaymentDate = p.PaymentDate,
                    CustomerName = p.Customer!.Name,
                    InvoiceNumber = p.SalesInvoice!.InvoiceNumber,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new SalesInvoicePaymentViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var payment = await _db.SalesInvoicePayments
                .Include(p => p.Customer)
                .Include(p => p.SalesInvoice)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "SALES INVOICE PAYMENT RECEIPT",
                DocumentNumberLabel = "RECEIPT #",
                DocumentNumber = payment.PaymentNumber,
                Status = payment.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", payment.PaymentDate.ToString("dd-MM-yyyy")),
                    ("INVOICE #", payment.SalesInvoice?.InvoiceNumber ?? "—"),
                    ("METHOD", payment.PaymentMethod.ToString()),
                    ("CHEQUE #", payment.ChequeNumber ?? "—"),
                    ("BANK", payment.BankName ?? "—")
                },
                PartyLabel = "RECEIVED FROM (CUSTOMER)",
                PartyCode = payment.Customer?.CustomerCode,
                PartyVendorCode = payment.Customer?.OurVendorCodeAtCustomer,
                PartyName = payment.Customer?.Name,
                PartyAddress = payment.Customer?.Address,
                PartyEmail = payment.Customer?.Email,
                PartyPhone = payment.Customer?.Phone,
                PartyNTN = payment.Customer?.NTNNumber,
                PartySTRN = payment.Customer?.SalesTaxRegistrationNumber,
                Columns = new() { "Description", "Amount (Rs.)" },
                Rows = new() { new() { $"Payment received against invoice {payment.SalesInvoice?.InvoiceNumber}", payment.Amount.ToString("N2") } },
                Totals = payment.WithholdingTaxRate.HasValue && payment.WithholdingTaxAmount > 0
                    ? new()
                    {
                        ("Amount", payment.Amount.ToString("N2"), false),
                        ($"Withholding Tax ({payment.WithholdingTaxRate.Value:0.##}%)", $"- {payment.WithholdingTaxAmount:N2}", false),
                        ("Net Amount Received", payment.NetReceivedAmount.ToString("N2"), true)
                    }
                    : new() { ("Amount Received", payment.Amount.ToString("N2"), true) },
                AmountInWords = NumberToWordsConverter.ToWords(payment.Amount),
                AdditionalNotes = payment.Remarks,
                ApprovedByName = payment.ApprovedByName,
                ApprovedOn = payment.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StandardUser}")]
        public async Task<IActionResult> Save(SalesInvoicePaymentViewModel model)
        {
            if (model.PaymentMethod == PaymentMethod.Cheque && string.IsNullOrWhiteSpace(model.ChequeNumber))
                ModelState.AddModelError(nameof(model.ChequeNumber), "Enter the cheque number for this payment.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var withholdingTaxAmount = model.WithholdingTaxRate.HasValue
                ? Math.Round(model.Amount * model.WithholdingTaxRate.Value / 100m, 2)
                : 0m;

            var payment = new SalesInvoicePayment
            {
                PaymentNumber = await _stockService.GenerateDocumentNumberAsync("SPAY", _db.SalesInvoicePayments.Select(p => p.PaymentNumber)),
                PaymentDate = model.PaymentDate,
                CustomerId = model.CustomerId,
                SalesInvoiceId = model.SalesInvoiceId,
                Amount = model.Amount,
                WithholdingTaxRate = model.WithholdingTaxRate,
                WithholdingTaxAmount = withholdingTaxAmount,
                NetReceivedAmount = model.Amount - withholdingTaxAmount,
                PaymentMethod = model.PaymentMethod,
                ChequeNumber = model.PaymentMethod == PaymentMethod.Cheque ? model.ChequeNumber : null,
                BankName = model.PaymentMethod == PaymentMethod.Cheque ? model.BankName : null,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            _db.SalesInvoicePayments.Add(payment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payment {payment.PaymentNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var payment = await _db.SalesInvoicePayments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return Json(new { success = false, message = "Payment not found." });
            if (payment.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending payments can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            payment.Status = VoucherStatus.Approved;
            payment.ApprovedById = currentUser?.Id;
            payment.ApprovedByName = currentUser?.FullName;
            payment.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Payment {payment.PaymentNumber} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var payment = await _db.SalesInvoicePayments.FindAsync(request.Id);
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
            var payment = await _db.SalesInvoicePayments.FindAsync(id);
            if (payment == null) return Json(new { success = false, message = "Payment not found." });

            if (payment.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) payments can be deleted." });

            _db.SalesInvoicePayments.Remove(payment);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Payment deleted." });
        }

        /// <summary>Feeds the invoice dropdown with each invoice's outstanding balance for quick selection.</summary>
        [HttpGet("GetInvoicesForCustomer/{customerId:int}")]
        public async Task<IActionResult> GetInvoicesForCustomer(int customerId)
        {
            var invoices = await _db.SalesInvoices
                .Where(i => i.CustomerId == customerId && i.Status == VoucherStatus.Approved)
                .ToListAsync();

            var paidByInvoice = await _db.SalesInvoicePayments
                .Where(p => p.Status == VoucherStatus.Approved && p.SalesInvoice!.CustomerId == customerId)
                .GroupBy(p => p.SalesInvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            var result = invoices.Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.CustomerId,
                Balance = i.TotalAmount - (paidByInvoice.TryGetValue(i.Id, out var paid) ? paid : 0)
            }).Where(i => i.Balance > 0);

            return Json(result);
        }

        private async Task PopulateDropdownsAsync(SalesInvoicePaymentViewModel vm)
        {
            vm.Customers = await _db.Customers.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CustomerOption(c.Id, c.Name, c.CustomerCode))
                .ToListAsync();

            var approvedInvoices = await _db.SalesInvoices.Where(i => i.Status == VoucherStatus.Approved).ToListAsync();
            var paidByInvoice = await _db.SalesInvoicePayments
                .Where(p => p.Status == VoucherStatus.Approved)
                .GroupBy(p => p.SalesInvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            vm.Invoices = approvedInvoices
                .Select(i => new SalesInvoiceOption(i.Id, i.InvoiceNumber, i.TotalAmount - (paidByInvoice.TryGetValue(i.Id, out var paid) ? paid : 0), i.CustomerId))
                .Where(i => i.Balance > 0)
                .ToList();
        }
    }
}
