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
    [Route("Sales/Orders")]
    public class SaleOrdersController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public SaleOrdersController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.SaleOrders
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .OrderByDescending(s => s.Id)
                .Select(s => new SaleOrderListItemViewModel
                {
                    Id = s.Id,
                    SONumber = s.SONumber,
                    SODate = s.SODate,
                    CustomerName = s.Customer!.Name,
                    TotalAmount = s.TotalAmount,
                    ItemCount = s.Items.Count,
                    Status = s.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new SaleOrderViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var so = await _db.SaleOrders
                .Include(s => s.Customer)
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (so == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View(so);
        }

        /// <summary>Feeds auto-populate on Delivery Challan / Invoice forms when a SO is selected.</summary>
        [HttpGet("GetDetails/{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var so = await _db.SaleOrders
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (so == null) return NotFound();

            return Json(new
            {
                saleOrderId = so.Id,
                customerId = so.CustomerId,
                customerPoNumbers = so.CustomerPONumbers,
                items = so.Items.Select(i => new
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
            var so = await _db.SaleOrders
                .Include(s => s.Customer)
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (so == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "SALE ORDER",
                DocumentNumberLabel = "SO #",
                DocumentNumber = so.SONumber,
                Status = so.Status.ToString(),
                ExtraRefs = new()
                {
                    ("SO DATE", so.SODate.ToString("dd-MM-yyyy")),
                    ("EXPECTED DELIVERY", so.ExpectedDeliveryDate?.ToString("dd-MM-yyyy") ?? "—"),
                    ("CUSTOMER PO #(S)", so.CustomerPONumbers ?? "—")
                },
                PartyLabel = "CUSTOMER",
                PartyCode = so.Customer?.CustomerCode,
                PartyVendorCode = so.Customer?.OurVendorCodeAtCustomer,
                PartyName = so.Customer?.Name,
                PartyAddress = so.Customer?.Address,
                PartyEmail = so.Customer?.Email,
                PartyPhone = so.Customer?.Phone,
                PartyNTN = so.Customer?.NTNNumber,
                PartySTRN = so.Customer?.SalesTaxRegistrationNumber,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Qty", "Unit Price (Rs.)", "Total (Rs.)" },
                Rows = so.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "",
                    i.Quantity.ToString("N2"), i.UnitPrice.ToString("N2"), i.LineTotal.ToString("N2")
                }).ToList(),
                Totals = new()
                {
                    ("Total Quantity", so.Items.Sum(i => i.Quantity).ToString("N2"), false),
                    ("Total Amount", so.TotalAmount.ToString("N2"), true)
                },
                AmountInWords = NumberToWordsConverter.ToWords(so.TotalAmount),
                AdditionalNotes = so.Remarks,
                ApprovedByName = so.ApprovedByName,
                ApprovedOn = so.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StandardUser}")]
        public async Task<IActionResult> Save(SaleOrderViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var so = new SaleOrder
            {
                SONumber = await _stockService.GenerateDocumentNumberAsync("SO", _db.SaleOrders.Select(s => s.SONumber)),
                SODate = model.SODate,
                CustomerId = model.CustomerId,
                ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                Remarks = model.Remarks,
                CustomerPONumbers = model.CustomerPONumbers,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                var lineTotal = item.Quantity * item.UnitPrice;
                if (item.SalesTaxRate.HasValue) lineTotal += lineTotal * item.SalesTaxRate.Value / 100m;

                so.Items.Add(new SaleOrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SalesTaxRate = item.SalesTaxRate,
                    LineTotal = lineTotal,
                    Remarks = item.Remarks
                });
            }

            so.TotalAmount = so.Items.Sum(i => i.LineTotal);

            _db.SaleOrders.Add(so);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Sale order {so.SONumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var so = await _db.SaleOrders.FirstOrDefaultAsync(s => s.Id == id);
            if (so == null) return Json(new { success = false, message = "Sale order not found." });
            if (so.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending sale orders can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            so.Status = VoucherStatus.Approved;
            so.ApprovedById = currentUser?.Id;
            so.ApprovedByName = currentUser?.FullName;
            so.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Sale order {so.SONumber} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var so = await _db.SaleOrders.FindAsync(request.Id);
            if (so == null) return Json(new { success = false, message = "Sale order not found." });
            if (so.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending sale orders can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            so.Status = VoucherStatus.Rejected;
            so.ApprovedById = currentUser?.Id;
            so.ApprovedByName = currentUser?.FullName;
            so.ApprovedOn = DateTime.UtcNow;
            so.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Sale order {so.SONumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var so = await _db.SaleOrders.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
            if (so == null) return Json(new { success = false, message = "Sale order not found." });

            if (so.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) sale orders can be deleted." });

            _db.SaleOrderItems.RemoveRange(so.Items);
            _db.SaleOrders.Remove(so);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Sale order deleted." });
        }

        private async Task PopulateDropdownsAsync(SaleOrderViewModel vm)
        {
            vm.Customers = await _db.Customers.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CustomerOption(c.Id, c.Name, c.CustomerCode))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Saleable | ProductType.Service)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new SaleOrderItemViewModel());
        }
    }
}
