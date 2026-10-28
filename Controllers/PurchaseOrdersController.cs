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
    [Route("Purchase/Orders")]
    public class PurchaseOrdersController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public PurchaseOrdersController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.PurchaseOrders
                .Include(p => p.Vendor)
                .Include(p => p.Items)
                .OrderByDescending(p => p.Id)
                .Select(p => new PurchaseOrderListItemViewModel
                {
                    Id = p.Id,
                    PONumber = p.PONumber,
                    PODate = p.PODate,
                    VendorName = p.Vendor!.Name,
                    TotalAmount = p.TotalAmount,
                    ItemCount = p.Items.Count,
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new PurchaseOrderViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        /// <summary>Feeds auto-populate on Gate Pass / GRN / Invoice forms when a PO is selected.</summary>
        [HttpGet("GetDetails/{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var po = await _db.PurchaseOrders
                .Include(p => p.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (po == null) return NotFound();

            return Json(new
            {
                vendorId = po.VendorId,
                items = po.Items.Select(i => new
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
            var po = await _db.PurchaseOrders
                .Include(p => p.Vendor)
                .Include(p => p.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (po == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "PURCHASE ORDER",
                DocumentNumberLabel = "PO #",
                DocumentNumber = po.PONumber,
                Status = po.Status.ToString(),
                ExtraRefs = new()
                {
                    ("PO DATE", po.PODate.ToString("dd-MM-yyyy")),
                    ("EXPECTED DELIVERY", po.ExpectedDeliveryDate?.ToString("dd-MM-yyyy") ?? "—")
                },
                PartyLabel = "VENDOR",
                PartyCode = po.Vendor?.VendorCode,
                PartyName = po.Vendor?.Name,
                PartyAddress = po.Vendor?.Address,
                PartyEmail = po.Vendor?.Email,
                PartyPhone = po.Vendor?.Phone,
                PartyNTN = po.Vendor?.NTNNumber,
                PartySTRN = po.Vendor?.SalesTaxRegistrationNumber,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Qty", "Unit Price (Rs.)", "Total (Rs.)" },
                Rows = po.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "",
                    i.Quantity.ToString("N2"), i.UnitPrice.ToString("N2"), i.LineTotal.ToString("N2")
                }).ToList(),
                Totals = new()
                {
                    ("Total Quantity", po.Items.Sum(i => i.Quantity).ToString("N2"), false),
                    ("Total Amount", po.TotalAmount.ToString("N2"), true)
                },
                AmountInWords = NumberToWordsConverter.ToWords(po.TotalAmount),
                AdditionalNotes = po.Remarks,
                ApprovedByName = po.ApprovedByName,
                ApprovedOn = po.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var po = await _db.PurchaseOrders
                .Include(p => p.Vendor)
                .Include(p => p.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (po == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View(po);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(PurchaseOrderViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var po = new PurchaseOrder
            {
                PONumber = await _stockService.GenerateDocumentNumberAsync("PO", _db.PurchaseOrders.Select(p => p.PONumber)),
                PODate = model.PODate,
                VendorId = model.VendorId,
                ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                var lineTotal = item.Quantity * item.UnitPrice;
                if (item.SalesTaxRate.HasValue) lineTotal += lineTotal * item.SalesTaxRate.Value / 100m;

                po.Items.Add(new PurchaseOrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SalesTaxRate = item.SalesTaxRate,
                    LineTotal = lineTotal,
                    Remarks = item.Remarks
                });
            }

            po.TotalAmount = po.Items.Sum(i => i.LineTotal);

            _db.PurchaseOrders.Add(po);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Purchase order {po.PONumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var po = await _db.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == id);
            if (po == null) return Json(new { success = false, message = "Purchase order not found." });
            if (po.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending purchase orders can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            po.Status = VoucherStatus.Approved;
            po.ApprovedById = currentUser?.Id;
            po.ApprovedByName = currentUser?.FullName;
            po.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Purchase order {po.PONumber} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var po = await _db.PurchaseOrders.FindAsync(request.Id);
            if (po == null) return Json(new { success = false, message = "Purchase order not found." });
            if (po.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending purchase orders can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            po.Status = VoucherStatus.Rejected;
            po.ApprovedById = currentUser?.Id;
            po.ApprovedByName = currentUser?.FullName;
            po.ApprovedOn = DateTime.UtcNow;
            po.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Purchase order {po.PONumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var po = await _db.PurchaseOrders.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
            if (po == null) return Json(new { success = false, message = "Purchase order not found." });

            if (po.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) purchase orders can be deleted." });

            _db.PurchaseOrderItems.RemoveRange(po.Items);
            _db.PurchaseOrders.Remove(po);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Purchase order deleted." });
        }

        private async Task PopulateDropdownsAsync(PurchaseOrderViewModel vm)
        {
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new VendorOption(v.Id, v.Name, v.VendorCode))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Purchasable | ProductType.Consumable)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new PurchaseOrderItemViewModel());
        }
    }
}
