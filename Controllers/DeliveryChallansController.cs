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
    [Route("Sales/DeliveryChallans")]
    public class DeliveryChallansController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.StoreIncharge}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public DeliveryChallansController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.DeliveryChallans
                .Include(d => d.Customer)
                .Include(d => d.Location)
                .Include(d => d.Items)
                .OrderByDescending(d => d.Id)
                .Select(d => new DeliveryChallanListItemViewModel
                {
                    Id = d.Id,
                    ChallanNo = d.ChallanNo,
                    ChallanDate = d.ChallanDate,
                    CustomerName = d.Customer!.Name,
                    LocationName = d.Location!.Name,
                    ItemCount = d.Items.Count,
                    Status = d.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new DeliveryChallanViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var challan = await _db.DeliveryChallans
                .Include(d => d.Customer)
                .Include(d => d.Location)
                .Include(d => d.SaleOrder)
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (challan == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.StoreIncharge);
            return View(challan);
        }

        /// <summary>Feeds auto-populate on the Invoice form when a Delivery Challan is selected.</summary>
        [HttpGet("GetDetails/{id:int}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var challan = await _db.DeliveryChallans
                .Include(d => d.SaleOrder)
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (challan == null) return NotFound();

            return Json(new
            {
                customerId = challan.CustomerId,
                saleOrderId = challan.SaleOrderId,
                customerPoNumbers = challan.SaleOrder?.CustomerPONumbers,
                items = challan.Items.Select(i => new
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
            var challan = await _db.DeliveryChallans
                .Include(d => d.Customer)
                .Include(d => d.SaleOrder)
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (challan == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "DELIVERY CHALLAN",
                DocumentNumberLabel = "CHALLAN #",
                DocumentNumber = challan.ChallanNo,
                Status = challan.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", challan.ChallanDate.ToString("dd-MM-yyyy")),
                    ("SO #", challan.SaleOrder?.SONumber ?? "—"),
                    ("VEHICLE #", challan.VehicleNo ?? "—"),
                    ("DRIVER", challan.DriverName ?? "—")
                },
                PartyLabel = "CUSTOMER",
                PartyCode = challan.Customer?.CustomerCode,
                PartyVendorCode = challan.Customer?.OurVendorCodeAtCustomer,
                PartyName = challan.Customer?.Name,
                PartyAddress = challan.Customer?.Address,
                PartyEmail = challan.Customer?.Email,
                PartyPhone = challan.Customer?.Phone,
                PartyNTN = challan.Customer?.NTNNumber,
                PartySTRN = challan.Customer?.SalesTaxRegistrationNumber,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Quantity", "Unit Price (Rs.)" },
                Rows = challan.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "", i.Quantity.ToString("N2"), i.UnitPrice?.ToString("N2") ?? "—"
                }).ToList(),
                Totals = new() { ("Total Quantity", challan.Items.Sum(i => i.Quantity).ToString("N2"), true) },
                AdditionalNotes = challan.Remarks,
                ApprovedByName = challan.ApprovedByName,
                ApprovedOn = challan.ApprovedOn,
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.Supervisor}")]
        public async Task<IActionResult> Save(DeliveryChallanViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var challan = new DeliveryChallan
            {
                ChallanNo = await _stockService.GenerateDocumentNumberAsync("DC", _db.DeliveryChallans.Select(d => d.ChallanNo)),
                ChallanDate = model.ChallanDate,
                CustomerId = model.CustomerId,
                SaleOrderId = model.SaleOrderId,
                LocationId = model.LocationId,
                VehicleNo = model.VehicleNo,
                DriverName = model.DriverName,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                challan.Items.Add(new DeliveryChallanItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Remarks = item.Remarks
                });
            }

            _db.DeliveryChallans.Add(challan);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Delivery challan {challan.ChallanNo} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var challan = await _db.DeliveryChallans.Include(d => d.Items).FirstOrDefaultAsync(d => d.Id == id);
            if (challan == null) return Json(new { success = false, message = "Delivery challan not found." });
            if (challan.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending delivery challans can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            challan.Status = VoucherStatus.Approved;
            challan.ApprovedById = currentUser?.Id;
            challan.ApprovedByName = currentUser?.FullName;
            challan.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Goods physically leave the warehouse — post to the shared stock ledger.
            foreach (var item in challan.Items)
            {
                await _stockService.PostStockMovementAsync(
                    item.ProductId, challan.LocationId, -Math.Abs(item.Quantity), StockTransactionType.SalesDelivery,
                    "DeliveryChallan", challan.Id, challan.ChallanNo, item.Remarks, challan.ChallanDate);
            }

            return Json(new { success = true, message = $"Delivery challan {challan.ChallanNo} approved and posted to stock." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var challan = await _db.DeliveryChallans.FindAsync(request.Id);
            if (challan == null) return Json(new { success = false, message = "Delivery challan not found." });
            if (challan.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending delivery challans can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            challan.Status = VoucherStatus.Rejected;
            challan.ApprovedById = currentUser?.Id;
            challan.ApprovedByName = currentUser?.FullName;
            challan.ApprovedOn = DateTime.UtcNow;
            challan.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Delivery challan {challan.ChallanNo} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var challan = await _db.DeliveryChallans.Include(d => d.Items).FirstOrDefaultAsync(d => d.Id == id);
            if (challan == null) return Json(new { success = false, message = "Delivery challan not found." });

            if (challan.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) delivery challans can be deleted." });

            _db.DeliveryChallanItems.RemoveRange(challan.Items);
            _db.DeliveryChallans.Remove(challan);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Delivery challan deleted." });
        }

        private async Task PopulateDropdownsAsync(DeliveryChallanViewModel vm)
        {
            vm.Customers = await _db.Customers.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CustomerOption(c.Id, c.Name, c.CustomerCode))
                .ToListAsync();

            vm.SaleOrders = await _db.SaleOrders.Where(s => s.Status == VoucherStatus.Approved)
                .OrderByDescending(s => s.Id)
                .Select(s => new SaleOrderOption(s.Id, s.SONumber))
                .ToListAsync();

            vm.Locations = await _db.Locations.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocationOption(l.Id, l.Name))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Saleable | ProductType.Service)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new DeliveryChallanItemViewModel());
        }
    }
}
