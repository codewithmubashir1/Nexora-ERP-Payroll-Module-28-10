using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Inventory/Consumption")]
    public class ConsumptionVouchersController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Supervisor}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public ConsumptionVouchersController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.ConsumptionVouchers
                .Include(v => v.Location)
                .Include(v => v.Items)
                .OrderByDescending(v => v.Id)
                .Select(v => new ConsumptionVoucherListItemViewModel
                {
                    Id = v.Id,
                    VoucherNo = v.VoucherNo,
                    VoucherDate = v.VoucherDate,
                    Location = v.Location!.Name,
                    IssuedTo = v.IssuedTo,
                    ItemCount = v.Items.Count,
                    Status = v.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new ConsumptionVoucherViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var voucher = await _db.ConsumptionVouchers
                .Include(v => v.Location)
                .Include(v => v.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (voucher == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "CONSUMPTION VOUCHER",
                DocumentNumberLabel = "VOUCHER #",
                DocumentNumber = voucher.VoucherNo,
                Status = voucher.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", voucher.VoucherDate.ToString("dd-MM-yyyy")),
                    ("LOCATION", voucher.Location?.Name ?? "—"),
                    ("DEPARTMENT", voucher.Department ?? "—"),
                    ("ISSUED TO", voucher.IssuedTo ?? "—")
                },
                PartyLabel = "ISSUED TO",
                PartyName = voucher.IssuedTo ?? voucher.Department,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Quantity", "Remarks" },
                Rows = voucher.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "", i.Quantity.ToString("N2"), i.Remarks ?? "—"
                }).ToList(),
                Totals = new() { ("Total Quantity", voucher.Items.Sum(i => i.Quantity).ToString("N2"), true) },
                AdditionalNotes = voucher.Purpose ?? voucher.Remarks,
                ApprovedByName = voucher.ApprovedByName,
                ApprovedOn = voucher.ApprovedOn,
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var voucher = await _db.ConsumptionVouchers
                .Include(v => v.Location)
                .Include(v => v.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (voucher == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Supervisor);
            return View(voucher);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(ConsumptionVoucherViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var voucher = new ConsumptionVoucher
            {
                VoucherNo = await _stockService.GenerateDocumentNumberAsync("CV", _db.ConsumptionVouchers.Select(v => v.VoucherNo)),
                VoucherDate = model.VoucherDate,
                LocationId = model.LocationId,
                Department = model.Department,
                IssuedTo = model.IssuedTo,
                Purpose = model.Purpose,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                voucher.Items.Add(new ConsumptionVoucherItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Remarks = item.Remarks
                });
            }

            _db.ConsumptionVouchers.Add(voucher);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Consumption voucher {voucher.VoucherNo} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var voucher = await _db.ConsumptionVouchers.Include(v => v.Items).FirstOrDefaultAsync(v => v.Id == id);
            if (voucher == null) return Json(new { success = false, message = "Voucher not found." });
            if (voucher.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending vouchers can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);

            voucher.Status = VoucherStatus.Approved;
            voucher.ApprovedById = currentUser?.Id;
            voucher.ApprovedByName = currentUser?.FullName;
            voucher.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _stockService.PostConsumptionAsync(voucher);

            return Json(new { success = true, message = $"Voucher {voucher.VoucherNo} approved and posted to stock." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var voucher = await _db.ConsumptionVouchers.FindAsync(request.Id);
            if (voucher == null) return Json(new { success = false, message = "Voucher not found." });
            if (voucher.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending vouchers can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);

            voucher.Status = VoucherStatus.Rejected;
            voucher.ApprovedById = currentUser?.Id;
            voucher.ApprovedByName = currentUser?.FullName;
            voucher.ApprovedOn = DateTime.UtcNow;
            voucher.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Voucher {voucher.VoucherNo} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _db.ConsumptionVouchers.Include(v => v.Items).FirstOrDefaultAsync(v => v.Id == id);
            if (voucher == null) return Json(new { success = false, message = "Voucher not found." });

            if (voucher.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) vouchers can be deleted." });

            _db.ConsumptionVoucherItems.RemoveRange(voucher.Items);
            _db.ConsumptionVouchers.Remove(voucher);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Voucher deleted." });
        }

        private async Task PopulateDropdownsAsync(ConsumptionVoucherViewModel vm)
        {
            vm.Locations = await _db.Locations.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocationOption(l.Id, l.Name))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new ConsumptionVoucherItemViewModel());
        }
    }
}
