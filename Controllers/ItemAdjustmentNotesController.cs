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
    [Route("Inventory/Adjustments")]
    public class ItemAdjustmentNotesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Supervisor}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public ItemAdjustmentNotesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.ItemAdjustmentNotes
                .Include(n => n.Location)
                .Include(n => n.Items)
                .OrderByDescending(n => n.Id)
                .Select(n => new ItemAdjustmentNoteListItemViewModel
                {
                    Id = n.Id,
                    AdjustmentNo = n.AdjustmentNo,
                    AdjustmentDate = n.AdjustmentDate,
                    Location = n.Location!.Name,
                    Reason = n.Reason,
                    ItemCount = n.Items.Count,
                    Status = n.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new ItemAdjustmentNoteViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var note = await _db.ItemAdjustmentNotes
                .Include(n => n.Location)
                .Include(n => n.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null) return NotFound();

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "ITEM ADJUSTMENT NOTE",
                DocumentNumberLabel = "ADJUSTMENT #",
                DocumentNumber = note.AdjustmentNo,
                Status = note.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", note.AdjustmentDate.ToString("dd-MM-yyyy")),
                    ("LOCATION", note.Location?.Name ?? "—"),
                    ("REASON", note.Reason ?? "—")
                },
                PartyLabel = "LOCATION",
                PartyName = note.Location?.Name,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Type", "Quantity", "Remarks" },
                Rows = note.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "",
                    i.AdjustmentType.ToString(), i.Quantity.ToString("N2"), i.Remarks ?? "—"
                }).ToList(),
                Totals = new() { ("Total Lines", note.Items.Count.ToString(), true) },
                AdditionalNotes = note.Remarks,
                ApprovedByName = note.ApprovedByName,
                ApprovedOn = note.ApprovedOn,
                ShowBankDetails = false,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var note = await _db.ItemAdjustmentNotes
                .Include(n => n.Location)
                .Include(n => n.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Supervisor);
            return View(note);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(ItemAdjustmentNoteViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var note = new ItemAdjustmentNote
            {
                AdjustmentNo = await _stockService.GenerateDocumentNumberAsync("ADJ", _db.ItemAdjustmentNotes.Select(n => n.AdjustmentNo)),
                AdjustmentDate = model.AdjustmentDate,
                LocationId = model.LocationId,
                Reason = model.Reason,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                note.Items.Add(new ItemAdjustmentNoteItem
                {
                    ProductId = item.ProductId,
                    AdjustmentType = item.AdjustmentType,
                    Quantity = item.Quantity,
                    Remarks = item.Remarks
                });
            }

            _db.ItemAdjustmentNotes.Add(note);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Adjustment note {note.AdjustmentNo} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var note = await _db.ItemAdjustmentNotes.Include(n => n.Items).FirstOrDefaultAsync(n => n.Id == id);
            if (note == null) return Json(new { success = false, message = "Adjustment note not found." });
            if (note.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending notes can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);

            note.Status = VoucherStatus.Approved;
            note.ApprovedById = currentUser?.Id;
            note.ApprovedByName = currentUser?.FullName;
            note.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _stockService.PostAdjustmentAsync(note);

            return Json(new { success = true, message = $"Adjustment {note.AdjustmentNo} approved and posted to stock." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var note = await _db.ItemAdjustmentNotes.FindAsync(request.Id);
            if (note == null) return Json(new { success = false, message = "Adjustment note not found." });
            if (note.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending notes can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);

            note.Status = VoucherStatus.Rejected;
            note.ApprovedById = currentUser?.Id;
            note.ApprovedByName = currentUser?.FullName;
            note.ApprovedOn = DateTime.UtcNow;
            note.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Adjustment {note.AdjustmentNo} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _db.ItemAdjustmentNotes.Include(n => n.Items).FirstOrDefaultAsync(n => n.Id == id);
            if (note == null) return Json(new { success = false, message = "Adjustment note not found." });

            if (note.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) notes can be deleted." });

            _db.ItemAdjustmentNoteItems.RemoveRange(note.Items);
            _db.ItemAdjustmentNotes.Remove(note);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Adjustment note deleted." });
        }

        private async Task PopulateDropdownsAsync(ItemAdjustmentNoteViewModel vm)
        {
            vm.Locations = await _db.Locations.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocationOption(l.Id, l.Name))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new ItemAdjustmentNoteItemViewModel());
        }
    }
}
