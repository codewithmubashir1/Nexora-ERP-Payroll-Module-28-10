using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
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
    [Route("Sales/Invoices")]
    public class SalesInvoicesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public SalesInvoicesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var invoices = await _db.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            var paidByInvoice = await _db.SalesInvoicePayments
                .Where(p => p.Status == VoucherStatus.Approved)
                .GroupBy(p => p.SalesInvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            var list = invoices.Select(s => new SalesInvoiceListItemViewModel
            {
                Id = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                InvoiceDate = s.InvoiceDate,
                CustomerName = s.Customer!.Name,
                TotalAmount = s.TotalAmount,
                PaidAmount = paidByInvoice.TryGetValue(s.Id, out var paid) ? paid : 0,
                ItemCount = s.Items.Count,
                Status = s.Status.ToString()
            }).ToList();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new SalesInvoiceViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var invoice = await _db.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.SaleOrder)
                .Include(s => s.DeliveryChallan)
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (invoice == null) return NotFound();

            ViewBag.PaidAmount = await _db.SalesInvoicePayments
                .Where(p => p.SalesInvoiceId == id && p.Status == VoucherStatus.Approved)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View(invoice);
        }

        /// <summary>Feeds auto-populate on the Credit Note form when a Sales Invoice is selected.</summary>
        [HttpGet("GetDetails/{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var invoice = await _db.SalesInvoices
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (invoice == null) return NotFound();

            return Json(new
            {
                customerId = invoice.CustomerId,
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

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var invoice = await _db.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.SaleOrder)
                .Include(s => s.DeliveryChallan)
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (invoice == null) return NotFound();

            var paidAmount = await _db.SalesInvoicePayments
                .Where(p => p.SalesInvoiceId == id && p.Status == VoucherStatus.Approved)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "SALES TAX INVOICE",
                DocumentNumberLabel = "SALES TAX INVOICE #",
                DocumentNumber = invoice.InvoiceNumber,
                Status = invoice.Status.ToString(),
                ExtraRefs = new()
                {
                    ("BILLING DATE", invoice.InvoiceDate.ToString("dd-MM-yyyy")),
                    ("DUE DATE", invoice.DueDate?.ToString("dd-MM-yyyy") ?? "—"),
                    ("PAYMENT TERMS", invoice.PaymentTermsDays.HasValue ? (invoice.PaymentTermsDays.Value == 0 ? "On Receipt" : $"{invoice.PaymentTermsDays.Value} Days") : "—"),
                    ("SO #", invoice.SaleOrder?.SONumber ?? "—"),
                    ("DELIVERY CHALLAN #", invoice.DeliveryChallan?.ChallanNo ?? "—"),
                    ("CUSTOMER PO #(S)", invoice.CustomerPONumbers ?? "—")
                },
                PartyLabel = "CUSTOMER",
                PartyCode = invoice.Customer?.CustomerCode,
                PartyVendorCode = invoice.Customer?.OurVendorCodeAtCustomer,
                PartyName = invoice.Customer?.Name,
                PartyAddress = invoice.Customer?.Address,
                PartyEmail = invoice.Customer?.Email,
                PartyPhone = invoice.Customer?.Phone,
                PartyNTN = invoice.Customer?.NTNNumber,
                PartySTRN = invoice.Customer?.SalesTaxRegistrationNumber,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "QTY", "Price (Rs.)", "Sub Total (Rs.)", "GST", "Amount (Rs.)" },
                Rows = invoice.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "",
                    i.Quantity.ToString("N2"), i.UnitPrice.ToString("N2"), (i.Quantity * i.UnitPrice).ToString("N2"),
                    i.TaxRate.HasValue ? $"{i.TaxRate.Value:0.##}%" : "—", i.LineTotal.ToString("N2")
                }).ToList(),
                Totals = new()
                {
                    ("Total Quantity", invoice.Items.Sum(i => i.Quantity).ToString("N2"), false),
                    ("Gross Total", invoice.SubTotal.ToString("N2"), false),
                    ("Tax On Product", invoice.TaxAmount.ToString("N2"), false),
                    ("Total Net Amount", invoice.TotalAmount.ToString("N2"), true),
                    ("Paid", paidAmount.ToString("N2"), false),
                    ("Balance", (invoice.TotalAmount - paidAmount).ToString("N2"), true)
                },
                AmountInWords = NumberToWordsConverter.ToWords(invoice.TotalAmount),
                TaxAmountInWords = invoice.TaxAmount > 0 ? NumberToWordsConverter.ToWords(invoice.TaxAmount) : null,
                AdditionalNotes = invoice.Remarks,
                ApprovedByName = invoice.ApprovedByName,
                ApprovedOn = invoice.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StandardUser}")]
        public async Task<IActionResult> Save(SalesInvoiceViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var invoice = new SalesInvoice
            {
                InvoiceNumber = await _stockService.GenerateDocumentNumberAsync("SINV", _db.SalesInvoices.Select(s => s.InvoiceNumber)),
                InvoiceDate = model.InvoiceDate,
                DueDate = model.PaymentTermsDays.HasValue
                    ? model.InvoiceDate.Date.AddDays(model.PaymentTermsDays.Value)
                    : model.DueDate,
                PaymentTermsDays = model.PaymentTermsDays,
                CustomerId = model.CustomerId,
                SaleOrderId = model.SaleOrderId,
                DeliveryChallanId = model.DeliveryChallanId,
                Remarks = model.Remarks,
                CustomerPONumbers = model.CustomerPONumbers,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                var subTotal = item.Quantity * item.UnitPrice;
                var taxAmount = item.TaxRate.HasValue ? subTotal * item.TaxRate.Value / 100m : 0m;

                invoice.Items.Add(new SalesInvoiceItem
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

            _db.SalesInvoices.Add(invoice);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Sales invoice {invoice.InvoiceNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var invoice = await _db.SalesInvoices.FirstOrDefaultAsync(s => s.Id == id);
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
            var invoice = await _db.SalesInvoices.FindAsync(request.Id);
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
            var invoice = await _db.SalesInvoices.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
            if (invoice == null) return Json(new { success = false, message = "Invoice not found." });

            if (invoice.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) invoices can be deleted." });

            var hasPayments = await _db.SalesInvoicePayments.AnyAsync(p => p.SalesInvoiceId == id);
            if (hasPayments)
                return Json(new { success = false, message = "This invoice has payments recorded against it and cannot be deleted." });

            _db.SalesInvoiceItems.RemoveRange(invoice.Items);
            _db.SalesInvoices.Remove(invoice);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Invoice deleted." });
        }

        private async Task PopulateDropdownsAsync(SalesInvoiceViewModel vm)
        {
            vm.Customers = await _db.Customers.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CustomerOption(c.Id, c.Name, c.CustomerCode, c.PaymentTermsDays))
                .ToListAsync();

            vm.SaleOrders = await _db.SaleOrders.Where(s => s.Status == VoucherStatus.Approved)
                .OrderByDescending(s => s.Id)
                .Select(s => new SaleOrderOption(s.Id, s.SONumber))
                .ToListAsync();

            vm.DeliveryChallans = await _db.DeliveryChallans.Where(d => d.Status == VoucherStatus.Approved)
                .OrderByDescending(d => d.Id)
                .Select(d => new DeliveryChallanOption(d.Id, d.ChallanNo))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Saleable | ProductType.Service)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new SalesInvoiceItemViewModel());
        }
    }
}
