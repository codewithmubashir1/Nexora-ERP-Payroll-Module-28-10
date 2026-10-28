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
    [Route("Purchase/GatePasses")]
    public class InwardGatePassesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.StoreIncharge}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public InwardGatePassesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.InwardGatePasses
                .Include(g => g.Vendor)
                .Include(g => g.Items)
                .OrderByDescending(g => g.Id)
                .Select(g => new InwardGatePassListItemViewModel
                {
                    Id = g.Id,
                    GatePassNo = g.GatePassNo,
                    GatePassDate = g.GatePassDate,
                    VendorName = g.Vendor!.Name,
                    VehicleNo = g.VehicleNo,
                    ItemCount = g.Items.Count,
                    Status = g.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new InwardGatePassViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        /// <summary>Feeds auto-populate on the GRN form when a Gate Pass is selected.</summary>
        [HttpGet("GetDetails/{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var gatePass = await _db.InwardGatePasses
                .Include(g => g.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gatePass == null) return NotFound();

            return Json(new
            {
                vendorId = gatePass.VendorId,
                purchaseOrderId = gatePass.PurchaseOrderId,
                items = gatePass.Items.Select(i => new
                {
                    productId = i.ProductId,
                    name = i.Product!.Name,
                    sku = i.Product.Sku,
                    declaredQuantity = i.DeclaredQuantity,
                    unitCost = i.UnitCost
                })
            });
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var gp = await _db.InwardGatePasses
                .Include(g => g.Vendor)
                .Include(g => g.PurchaseOrder)
                .Include(g => g.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gp == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "INWARD GATE PASS",
                DocumentNumberLabel = "GATE PASS #",
                DocumentNumber = gp.GatePassNo,
                Status = gp.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", gp.GatePassDate.ToString("dd-MM-yyyy")),
                    ("PO #", gp.PurchaseOrder?.PONumber ?? "—"),
                    ("VEHICLE #", gp.VehicleNo ?? "—"),
                    ("DRIVER", gp.DriverName ?? "—")
                },
                PartyLabel = "VENDOR",
                PartyCode = gp.Vendor?.VendorCode,
                PartyName = gp.Vendor?.Name,
                PartyAddress = gp.Vendor?.Address,
                PartyEmail = gp.Vendor?.Email,
                PartyPhone = gp.Vendor?.Phone,
                PartyNTN = gp.Vendor?.NTNNumber,
                PartySTRN = gp.Vendor?.SalesTaxRegistrationNumber,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Declared Qty", "Unit Cost (Rs.)" },
                Rows = gp.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "", i.DeclaredQuantity.ToString("N2"), i.UnitCost?.ToString("N2") ?? "—"
                }).ToList(),
                Totals = new() { ("Total Declared Qty", gp.Items.Sum(i => i.DeclaredQuantity).ToString("N2"), true) },
                AdditionalNotes = gp.Remarks,
                ApprovedByName = gp.ApprovedByName,
                ApprovedOn = gp.ApprovedOn,
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var gatePass = await _db.InwardGatePasses
                .Include(g => g.Vendor)
                .Include(g => g.PurchaseOrder)
                .Include(g => g.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gatePass == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.StoreIncharge);
            return View(gatePass);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.Supervisor}")]
        public async Task<IActionResult> Save(InwardGatePassViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var gatePass = new InwardGatePass
            {
                GatePassNo = await _stockService.GenerateDocumentNumberAsync("IGP", _db.InwardGatePasses.Select(g => g.GatePassNo)),
                GatePassDate = model.GatePassDate,
                VendorId = model.VendorId,
                PurchaseOrderId = model.PurchaseOrderId,
                VehicleNo = model.VehicleNo,
                DriverName = model.DriverName,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                gatePass.Items.Add(new InwardGatePassItem
                {
                    ProductId = item.ProductId,
                    DeclaredQuantity = item.DeclaredQuantity,
                    UnitCost = item.UnitCost,
                    Remarks = item.Remarks
                });
            }

            _db.InwardGatePasses.Add(gatePass);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Gate pass {gatePass.GatePassNo} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var gatePass = await _db.InwardGatePasses.FirstOrDefaultAsync(g => g.Id == id);
            if (gatePass == null) return Json(new { success = false, message = "Gate pass not found." });
            if (gatePass.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending gate passes can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            gatePass.Status = VoucherStatus.Approved;
            gatePass.ApprovedById = currentUser?.Id;
            gatePass.ApprovedByName = currentUser?.FullName;
            gatePass.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Gate pass {gatePass.GatePassNo} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var gatePass = await _db.InwardGatePasses.FindAsync(request.Id);
            if (gatePass == null) return Json(new { success = false, message = "Gate pass not found." });
            if (gatePass.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending gate passes can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            gatePass.Status = VoucherStatus.Rejected;
            gatePass.ApprovedById = currentUser?.Id;
            gatePass.ApprovedByName = currentUser?.FullName;
            gatePass.ApprovedOn = DateTime.UtcNow;
            gatePass.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Gate pass {gatePass.GatePassNo} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var gatePass = await _db.InwardGatePasses.Include(g => g.Items).FirstOrDefaultAsync(g => g.Id == id);
            if (gatePass == null) return Json(new { success = false, message = "Gate pass not found." });

            if (gatePass.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) gate passes can be deleted." });

            _db.InwardGatePassItems.RemoveRange(gatePass.Items);
            _db.InwardGatePasses.Remove(gatePass);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Gate pass deleted." });
        }

        private async Task PopulateDropdownsAsync(InwardGatePassViewModel vm)
        {
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new VendorOption(v.Id, v.Name, v.VendorCode))
                .ToListAsync();

            vm.PurchaseOrders = await _db.PurchaseOrders.Where(p => p.Status == VoucherStatus.Approved)
                .OrderByDescending(p => p.Id)
                .Select(p => new PurchaseOrderOption(p.Id, p.PONumber))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Purchasable | ProductType.Consumable)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new InwardGatePassItemViewModel());
        }
    }
}
