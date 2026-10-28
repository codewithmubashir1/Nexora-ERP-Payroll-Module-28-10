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
    [Route("Purchase/Invoices")]
    public class PurchaseInvoicesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public PurchaseInvoicesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var invoices = await _db.PurchaseInvoices
                .Include(p => p.Vendor)
                .Include(p => p.Items)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var paidByInvoice = await _db.PurchaseInvoicePayments
                .Where(p => p.Status == VoucherStatus.Approved)
                .GroupBy(p => p.PurchaseInvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            var list = invoices.Select(p => new PurchaseInvoiceListItemViewModel
            {
                Id = p.Id,
                InvoiceNumber = p.InvoiceNumber,
                InvoiceDate = p.InvoiceDate,
                VendorName = p.Vendor!.Name,
                TotalAmount = p.TotalAmount,
                NetPayableAmount = p.NetPayableAmount,
                PaidAmount = paidByInvoice.TryGetValue(p.Id, out var paid) ? paid : 0,
                ItemCount = p.Items.Count,
                Status = p.Status.ToString()
            }).ToList();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new PurchaseInvoiceViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var invoice = await _db.PurchaseInvoices
                .Include(p => p.Vendor)
                .Include(p => p.PurchaseOrder)
                .Include(p => p.GoodsReceiptNote)
                .Include(p => p.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (invoice == null) return NotFound();

            var paidAmount = await _db.PurchaseInvoicePayments
                .Where(p => p.PurchaseInvoiceId == id && p.Status == VoucherStatus.Approved)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var totals = new List<(string, string, bool)>
            {
                ("Sub Total", invoice.SubTotal.ToString("N2"), false),
                ("Sales Tax", invoice.TaxAmount.ToString("N2"), false),
                ("Gross Total", invoice.TotalAmount.ToString("N2"), false)
            };

            if (invoice.WithholdingTaxRate.HasValue && invoice.WithholdingTaxAmount > 0)
            {
                totals.Add(($"Withholding Tax ({invoice.WithholdingTaxRate.Value:0.##}%)", $"- {invoice.WithholdingTaxAmount:N2}", false));
                totals.Add(("Net Payable", invoice.NetPayableAmount.ToString("N2"), true));
            }
            else
            {
                totals.Add(("Total Net Amount", invoice.TotalAmount.ToString("N2"), true));
            }

            totals.Add(("Paid", paidAmount.ToString("N2"), false));
            totals.Add(("Balance", (invoice.NetPayableAmount - paidAmount).ToString("N2"), true));

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "PURCHASE INVOICE",
                DocumentNumberLabel = "INVOICE #",
                DocumentNumber = invoice.InvoiceNumber,
                Status = invoice.Status.ToString(),
                ExtraRefs = new()
                {
                    ("VENDOR INVOICE #", invoice.VendorInvoiceNumber ?? "—"),
                    ("INVOICE DATE", invoice.InvoiceDate.ToString("dd-MM-yyyy")),
                    ("DUE DATE", invoice.DueDate?.ToString("dd-MM-yyyy") ?? "—"),
                    ("PO #", invoice.PurchaseOrder?.PONumber ?? "—"),
                    ("GRN #", invoice.GoodsReceiptNote?.GRNNumber ?? "—")
                },
                PartyLabel = "VENDOR",
                PartyCode = invoice.Vendor?.VendorCode,
                PartyName = invoice.Vendor?.Name,
                PartyAddress = invoice.Vendor?.Address,
                PartyEmail = invoice.Vendor?.Email,
                PartyPhone = invoice.Vendor?.Phone,
                PartyNTN = invoice.Vendor?.NTNNumber,
                PartySTRN = invoice.Vendor?.SalesTaxRegistrationNumber,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Qty", "Price (Rs.)", "Sub Total (Rs.)", "Tax %", "Amount (Rs.)" },
                Rows = invoice.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "",
                    i.Quantity.ToString("N2"), i.UnitPrice.ToString("N2"), (i.Quantity * i.UnitPrice).ToString("N2"),
                    i.TaxRate.HasValue ? $"{i.TaxRate.Value:0.##}%" : "—", i.LineTotal.ToString("N2")
                }).ToList(),
                Totals = totals,
                AmountInWords = NumberToWordsConverter.ToWords(invoice.NetPayableAmount),
                TaxAmountInWords = invoice.TaxAmount > 0 ? NumberToWordsConverter.ToWords(invoice.TaxAmount) : null,
                AdditionalNotes = invoice.Remarks,
                ApprovedByName = invoice.ApprovedByName,
                ApprovedOn = invoice.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        /// <summary>Feeds auto-populate on the Debit Note form when a Purchase Invoice is selected.</summary>
        [HttpGet("GetDetails/{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var invoice = await _db.PurchaseInvoices
                .Include(p => p.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (invoice == null) return NotFound();

            return Json(new
            {
                vendorId = invoice.VendorId,
                items = invoice.Items.Select(i => new
                {
                    productId = i.ProductId,
                    name = i.Product!.Name,
                    sku = i.Product.Sku,
                    quantity = i.Quantity,
                    unitPrice = i.UnitPrice
                })
            });
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var invoice = await _db.PurchaseInvoices
                .Include(p => p.Vendor)
                .Include(p => p.PurchaseOrder)
                .Include(p => p.GoodsReceiptNote)
                .Include(p => p.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (invoice == null) return NotFound();

            ViewBag.PaidAmount = await _db.PurchaseInvoicePayments
                .Where(p => p.PurchaseInvoiceId == id && p.Status == VoucherStatus.Approved)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View(invoice);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(PurchaseInvoiceViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var invoice = new PurchaseInvoice
            {
                InvoiceNumber = await _stockService.GenerateDocumentNumberAsync("PINV", _db.PurchaseInvoices.Select(p => p.InvoiceNumber)),
                VendorInvoiceNumber = model.VendorInvoiceNumber,
                InvoiceDate = model.InvoiceDate,
                DueDate = model.PaymentTermsDays.HasValue
                    ? model.InvoiceDate.Date.AddDays(model.PaymentTermsDays.Value)
                    : model.DueDate,
                VendorId = model.VendorId,
                PurchaseOrderId = model.PurchaseOrderId,
                GoodsReceiptNoteId = model.GoodsReceiptNoteId,
                WithholdingTaxRate = model.WithholdingTaxRate,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                var subTotal = item.Quantity * item.UnitPrice;
                var taxAmount = item.TaxRate.HasValue ? subTotal * item.TaxRate.Value / 100m : 0m;

                invoice.Items.Add(new PurchaseInvoiceItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate,
                    LineTotal = subTotal + taxAmount,
                    Remarks = item.Remarks
                });
            }

            invoice.SubTotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
            invoice.TaxAmount = invoice.Items.Sum(i => i.LineTotal - (i.Quantity * i.UnitPrice));
            invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;
            invoice.WithholdingTaxAmount = model.WithholdingTaxRate.HasValue
                ? Math.Round(invoice.SubTotal * model.WithholdingTaxRate.Value / 100m, 2)
                : 0m;
            invoice.NetPayableAmount = invoice.TotalAmount - invoice.WithholdingTaxAmount;

            _db.PurchaseInvoices.Add(invoice);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Purchase invoice {invoice.InvoiceNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var invoice = await _db.PurchaseInvoices.FirstOrDefaultAsync(p => p.Id == id);
            if (invoice == null) return Json(new { success = false, message = "Invoice not found." });
            if (invoice.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending invoices can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            invoice.Status = VoucherStatus.Approved;
            invoice.ApprovedById = currentUser?.Id;
            invoice.ApprovedByName = currentUser?.FullName;
            invoice.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Invoice {invoice.InvoiceNumber} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var invoice = await _db.PurchaseInvoices.FindAsync(request.Id);
            if (invoice == null) return Json(new { success = false, message = "Invoice not found." });
            if (invoice.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending invoices can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            invoice.Status = VoucherStatus.Rejected;
            invoice.ApprovedById = currentUser?.Id;
            invoice.ApprovedByName = currentUser?.FullName;
            invoice.ApprovedOn = DateTime.UtcNow;
            invoice.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Invoice {invoice.InvoiceNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _db.PurchaseInvoices.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
            if (invoice == null) return Json(new { success = false, message = "Invoice not found." });

            if (invoice.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) invoices can be deleted." });

            var hasPayments = await _db.PurchaseInvoicePayments.AnyAsync(p => p.PurchaseInvoiceId == id);
            if (hasPayments)
                return Json(new { success = false, message = "This invoice has payments recorded against it and cannot be deleted." });

            _db.PurchaseInvoiceItems.RemoveRange(invoice.Items);
            _db.PurchaseInvoices.Remove(invoice);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Invoice deleted." });
        }

        private async Task PopulateDropdownsAsync(PurchaseInvoiceViewModel vm)
        {
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new PurchaseInvoiceVendorOption(v.Id, v.Name, v.VendorCode, v.PaymentTermsDays))
                .ToListAsync();

            vm.PurchaseOrders = await _db.PurchaseOrders.Where(p => p.Status == VoucherStatus.Approved)
                .OrderByDescending(p => p.Id)
                .Select(p => new PurchaseOrderOption(p.Id, p.PONumber))
                .ToListAsync();

            vm.GRNs = await _db.GoodsReceiptNotes.Where(g => g.Status == VoucherStatus.Approved)
                .OrderByDescending(g => g.Id)
                .Select(g => new GRNOption(g.Id, g.GRNNumber))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Purchasable | ProductType.Consumable)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new PurchaseInvoiceItemViewModel());
        }
    }
}
