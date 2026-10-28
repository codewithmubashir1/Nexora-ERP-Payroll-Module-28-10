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
    [Route("Inventory/Transfers")]
    public class InternalTransfersController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Supervisor}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public InternalTransfersController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.InternalTransfers
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .Include(t => t.Items)
                .OrderByDescending(t => t.Id)
                .Select(t => new InternalTransferListItemViewModel
                {
                    Id = t.Id,
                    TransferNo = t.TransferNo,
                    TransferDate = t.TransferDate,
                    FromLocation = t.FromLocation!.Name,
                    ToLocation = t.ToLocation!.Name,
                    ItemCount = t.Items.Count,
                    Status = t.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new InternalTransferViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var transfer = await _db.InternalTransfers
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .Include(t => t.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "INTERNAL TRANSFER",
                DocumentNumberLabel = "TRANSFER #",
                DocumentNumber = transfer.TransferNo,
                Status = transfer.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", transfer.TransferDate.ToString("dd-MM-yyyy")),
                    ("FROM", transfer.FromLocation?.Name ?? "—"),
                    ("TO", transfer.ToLocation?.Name ?? "—")
                },
                PartyLabel = "TRANSFER",
                PartyName = $"{transfer.FromLocation?.Name} \u2192 {transfer.ToLocation?.Name}",
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Quantity", "Remarks" },
                Rows = transfer.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "", i.Quantity.ToString("N2"), i.Remarks ?? "—"
                }).ToList(),
                Totals = new() { ("Total Quantity", transfer.Items.Sum(i => i.Quantity).ToString("N2"), true) },
                AdditionalNotes = transfer.Remarks,
                ApprovedByName = transfer.ApprovedByName,
                ApprovedOn = transfer.ApprovedOn,
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var transfer = await _db.InternalTransfers
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .Include(t => t.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Supervisor);
            return View(transfer);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(InternalTransferViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.FromLocationId == model.ToLocationId)
                ModelState.AddModelError(nameof(model.ToLocationId), "From and To locations must be different.");

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var transfer = new InternalTransfer
            {
                TransferNo = await _stockService.GenerateDocumentNumberAsync("TRF", _db.InternalTransfers.Select(t => t.TransferNo)),
                TransferDate = model.TransferDate,
                FromLocationId = model.FromLocationId,
                ToLocationId = model.ToLocationId,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                transfer.Items.Add(new InternalTransferItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Remarks = item.Remarks
                });
            }

            _db.InternalTransfers.Add(transfer);
            await _db.SaveChangesAsync();

            // Stock only moves once the transfer is approved — see Approve() below.
            TempData["SuccessMessage"] = $"Internal transfer {transfer.TransferNo} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var transfer = await _db.InternalTransfers.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == id);
            if (transfer == null) return Json(new { success = false, message = "Transfer not found." });
            if (transfer.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending transfers can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);

            transfer.Status = VoucherStatus.Approved;
            transfer.ApprovedById = currentUser?.Id;
            transfer.ApprovedByName = currentUser?.FullName;
            transfer.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _stockService.PostInternalTransferAsync(transfer);

            return Json(new { success = true, message = $"Transfer {transfer.TransferNo} approved and posted to stock." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var transfer = await _db.InternalTransfers.FindAsync(request.Id);
            if (transfer == null) return Json(new { success = false, message = "Transfer not found." });
            if (transfer.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending transfers can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);

            transfer.Status = VoucherStatus.Rejected;
            transfer.ApprovedById = currentUser?.Id;
            transfer.ApprovedByName = currentUser?.FullName;
            transfer.ApprovedOn = DateTime.UtcNow;
            transfer.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Transfer {transfer.TransferNo} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var transfer = await _db.InternalTransfers.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == id);
            if (transfer == null) return Json(new { success = false, message = "Transfer not found." });

            if (transfer.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) transfers can be deleted." });

            _db.InternalTransferItems.RemoveRange(transfer.Items);
            _db.InternalTransfers.Remove(transfer);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Transfer deleted." });
        }

        private async Task PopulateDropdownsAsync(InternalTransferViewModel vm)
        {
            vm.Locations = await _db.Locations.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocationOption(l.Id, l.Name))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new InternalTransferItemViewModel());
        }
    }
}
