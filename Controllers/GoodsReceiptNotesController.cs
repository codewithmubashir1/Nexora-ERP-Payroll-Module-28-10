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
    [Route("Purchase/GRN")]
    public class GoodsReceiptNotesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.StoreIncharge}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public GoodsReceiptNotesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.GoodsReceiptNotes
                .Include(g => g.Vendor)
                .Include(g => g.Location)
                .Include(g => g.Items)
                .OrderByDescending(g => g.Id)
                .Select(g => new GoodsReceiptNoteListItemViewModel
                {
                    Id = g.Id,
                    GRNNumber = g.GRNNumber,
                    GRNDate = g.GRNDate,
                    VendorName = g.Vendor!.Name,
                    LocationName = g.Location!.Name,
                    ItemCount = g.Items.Count,
                    Status = g.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new GoodsReceiptNoteViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        /// <summary>Feeds auto-populate on the Purchase Invoice form when a GRN is selected.</summary>
        [HttpGet("GetDetails/{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var grn = await _db.GoodsReceiptNotes
                .Include(g => g.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grn == null) return NotFound();

            return Json(new
            {
                vendorId = grn.VendorId,
                purchaseOrderId = grn.PurchaseOrderId,
                items = grn.Items.Where(i => i.AcceptedQuantity > 0).Select(i => new
                {
                    productId = i.ProductId,
                    name = i.Product!.Name,
                    sku = i.Product.Sku,
                    quantity = i.AcceptedQuantity,
                    unitPrice = i.UnitCost
                })
            });
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var grn = await _db.GoodsReceiptNotes
                .Include(g => g.Vendor)
                .Include(g => g.Location)
                .Include(g => g.PurchaseOrder)
                .Include(g => g.InwardGatePass)
                .Include(g => g.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grn == null) return NotFound();

            var acceptedTotal = grn.Items.Sum(i => i.LineTotal);

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "GOODS RECEIPT NOTE",
                DocumentNumberLabel = "GRN #",
                DocumentNumber = grn.GRNNumber,
                Status = grn.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", grn.GRNDate.ToString("dd-MM-yyyy")),
                    ("LOCATION", grn.Location?.Name ?? "—"),
                    ("PO #", grn.PurchaseOrder?.PONumber ?? "—"),
                    ("GATE PASS #", grn.InwardGatePass?.GatePassNo ?? "—")
                },
                PartyLabel = "VENDOR",
                PartyCode = grn.Vendor?.VendorCode,
                PartyName = grn.Vendor?.Name,
                PartyAddress = grn.Vendor?.Address,
                PartyEmail = grn.Vendor?.Email,
                PartyPhone = grn.Vendor?.Phone,
                PartyNTN = grn.Vendor?.NTNNumber,
                PartySTRN = grn.Vendor?.SalesTaxRegistrationNumber,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Declared", "Accepted", "Rejected", "Unit Cost", "Total (Rs.)" },
                Rows = grn.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "",
                    i.DeclaredQuantity.ToString("N2"), i.AcceptedQuantity.ToString("N2"), i.RejectedQuantity.ToString("N2"),
                    i.UnitCost.ToString("N2"), i.LineTotal.ToString("N2")
                }).ToList(),
                Totals = new()
                {
                    ("Accepted Qty", grn.Items.Sum(i => i.AcceptedQuantity).ToString("N2"), false),
                    ("Rejected Qty", grn.Items.Sum(i => i.RejectedQuantity).ToString("N2"), false),
                    ("Total Value (Accepted)", acceptedTotal.ToString("N2"), true)
                },
                AmountInWords = NumberToWordsConverter.ToWords(acceptedTotal),
                AdditionalNotes = grn.Remarks,
                ApprovedByName = grn.ApprovedByName,
                ApprovedOn = grn.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var grn = await _db.GoodsReceiptNotes
                .Include(g => g.Vendor)
                .Include(g => g.Location)
                .Include(g => g.PurchaseOrder)
                .Include(g => g.InwardGatePass)
                .Include(g => g.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grn == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.StoreIncharge);
            return View(grn);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.Supervisor}")]
        public async Task<IActionResult> Save(GoodsReceiptNoteViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var grn = new GoodsReceiptNote
            {
                GRNNumber = await _stockService.GenerateDocumentNumberAsync("GRN", _db.GoodsReceiptNotes.Select(g => g.GRNNumber)),
                GRNDate = model.GRNDate,
                VendorId = model.VendorId,
                PurchaseOrderId = model.PurchaseOrderId,
                InwardGatePassId = model.InwardGatePassId,
                LocationId = model.LocationId,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                grn.Items.Add(new GoodsReceiptNoteItem
                {
                    ProductId = item.ProductId,
                    DeclaredQuantity = item.DeclaredQuantity,
                    AcceptedQuantity = item.AcceptedQuantity,
                    RejectedQuantity = item.RejectedQuantity,
                    UnitCost = item.UnitCost,
                    LineTotal = item.AcceptedQuantity * item.UnitCost,
                    Remarks = item.Remarks
                });
            }

            _db.GoodsReceiptNotes.Add(grn);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Goods receipt note {grn.GRNNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var grn = await _db.GoodsReceiptNotes.Include(g => g.Items).FirstOrDefaultAsync(g => g.Id == id);
            if (grn == null) return Json(new { success = false, message = "GRN not found." });
            if (grn.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending GRNs can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            grn.Status = VoucherStatus.Approved;
            grn.ApprovedById = currentUser?.Id;
            grn.ApprovedByName = currentUser?.FullName;
            grn.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Post accepted quantities into the shared Inventory stock ledger — this is
            // the same StockTransaction table Internal Transfers/Adjustments/Consumption
            // post to, so current stock stays consistent everywhere in the app. Rejected
            // quantities never touch stock.
            foreach (var item in grn.Items.Where(i => i.AcceptedQuantity > 0))
            {
                await _stockService.PostStockMovementAsync(
                    item.ProductId, grn.LocationId, item.AcceptedQuantity, StockTransactionType.PurchaseReceipt,
                    "GoodsReceiptNote", grn.Id, grn.GRNNumber, item.Remarks, grn.GRNDate);
            }

            return Json(new { success = true, message = $"GRN {grn.GRNNumber} approved and posted to stock." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var grn = await _db.GoodsReceiptNotes.FindAsync(request.Id);
            if (grn == null) return Json(new { success = false, message = "GRN not found." });
            if (grn.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending GRNs can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            grn.Status = VoucherStatus.Rejected;
            grn.ApprovedById = currentUser?.Id;
            grn.ApprovedByName = currentUser?.FullName;
            grn.ApprovedOn = DateTime.UtcNow;
            grn.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"GRN {grn.GRNNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var grn = await _db.GoodsReceiptNotes.Include(g => g.Items).FirstOrDefaultAsync(g => g.Id == id);
            if (grn == null) return Json(new { success = false, message = "GRN not found." });

            if (grn.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) GRNs can be deleted." });

            _db.GoodsReceiptNoteItems.RemoveRange(grn.Items);
            _db.GoodsReceiptNotes.Remove(grn);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "GRN deleted." });
        }

        private async Task PopulateDropdownsAsync(GoodsReceiptNoteViewModel vm)
        {
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new VendorOption(v.Id, v.Name, v.VendorCode))
                .ToListAsync();

            vm.PurchaseOrders = await _db.PurchaseOrders.Where(p => p.Status == VoucherStatus.Approved)
                .OrderByDescending(p => p.Id)
                .Select(p => new PurchaseOrderOption(p.Id, p.PONumber))
                .ToListAsync();

            vm.GatePasses = await _db.InwardGatePasses.Where(g => g.Status == VoucherStatus.Approved)
                .OrderByDescending(g => g.Id)
                .Select(g => new GatePassOption(g.Id, g.GatePassNo))
                .ToListAsync();

            vm.Locations = await _db.Locations.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocationOption(l.Id, l.Name))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Purchasable | ProductType.Consumable)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new GoodsReceiptNoteItemViewModel());
        }
    }
}
