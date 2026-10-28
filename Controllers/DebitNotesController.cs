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
    [Route("Purchase/DebitNotes")]
    public class DebitNotesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public DebitNotesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.DebitNotes
                .Include(d => d.Vendor)
                .Include(d => d.Items)
                .OrderByDescending(d => d.Id)
                .Select(d => new DebitNoteListItemViewModel
                {
                    Id = d.Id,
                    DebitNoteNumber = d.DebitNoteNumber,
                    DebitNoteDate = d.DebitNoteDate,
                    VendorName = d.Vendor!.Name,
                    TotalAmount = d.TotalAmount,
                    Reason = d.Reason,
                    ItemCount = d.Items.Count,
                    Status = d.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new DebitNoteViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var note = await _db.DebitNotes
                .Include(d => d.Vendor)
                .Include(d => d.PurchaseInvoice)
                .Include(d => d.GoodsReceiptNote)
                .Include(d => d.Location)
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (note == null) return NotFound();

            var taxTotal = note.Items.Sum(i => i.LineTotal - (i.Quantity * i.UnitPrice));

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "DEBIT NOTE",
                DocumentNumberLabel = "DEBIT NOTE #",
                DocumentNumber = note.DebitNoteNumber,
                Status = note.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", note.DebitNoteDate.ToString("dd-MM-yyyy")),
                    ("REASON", note.Reason ?? "—"),
                    ("INVOICE #", note.PurchaseInvoice?.InvoiceNumber ?? "—"),
                    ("GRN #", note.GoodsReceiptNote?.GRNNumber ?? "—"),
                    ("RETURN LOCATION", note.Location?.Name ?? "Financial claim only")
                },
                PartyLabel = "VENDOR",
                PartyCode = note.Vendor?.VendorCode,
                PartyName = note.Vendor?.Name,
                PartyAddress = note.Vendor?.Address,
                PartyEmail = note.Vendor?.Email,
                PartyPhone = note.Vendor?.Phone,
                PartyNTN = note.Vendor?.NTNNumber,
                PartySTRN = note.Vendor?.SalesTaxRegistrationNumber,
                Columns = new() { "Sr.#", "Product Code", "Product Name", "Qty", "Unit Price (Rs.)", "Tax %", "Amount (Rs.)" },
                Rows = note.Items.Select((i, idx) => new List<string>
                {
                    (idx + 1).ToString(), i.Product?.ProductCode ?? i.Product?.Sku ?? "", i.Product?.Name ?? "",
                    i.Quantity.ToString("N2"), i.UnitPrice.ToString("N2"),
                    i.SalesTaxRate.HasValue ? $"{i.SalesTaxRate.Value:0.##}%" : "—", i.LineTotal.ToString("N2")
                }).ToList(),
                Totals = new()
                {
                    ("Sub Total", (note.TotalAmount - taxTotal).ToString("N2"), false),
                    ("Sales Tax", taxTotal.ToString("N2"), false),
                    ("Total Amount", note.TotalAmount.ToString("N2"), true)
                },
                AmountInWords = NumberToWordsConverter.ToWords(note.TotalAmount),
                AdditionalNotes = note.Remarks,
                ApprovedByName = note.ApprovedByName,
                ApprovedOn = note.ApprovedOn,
                Company = await _companyInfoService.GetAsync()
            };

            return View("~/Views/Shared/_PrintDocument.cshtml", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var note = await _db.DebitNotes
                .Include(d => d.Vendor)
                .Include(d => d.PurchaseInvoice)
                .Include(d => d.GoodsReceiptNote)
                .Include(d => d.Location)
                .Include(d => d.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (note == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View(note);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StoreIncharge}")]
        public async Task<IActionResult> Save(DebitNoteViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var note = new DebitNote
            {
                DebitNoteNumber = await _stockService.GenerateDocumentNumberAsync("DN", _db.DebitNotes.Select(d => d.DebitNoteNumber)),
                DebitNoteDate = model.DebitNoteDate,
                VendorId = model.VendorId,
                PurchaseInvoiceId = model.PurchaseInvoiceId,
                GoodsReceiptNoteId = model.GoodsReceiptNoteId,
                LocationId = model.LocationId,
                Reason = model.Reason,
                Remarks = model.Remarks,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var item in model.Items)
            {
                var lineTotal = item.Quantity * item.UnitPrice;
                if (item.SalesTaxRate.HasValue) lineTotal += lineTotal * item.SalesTaxRate.Value / 100m;

                note.Items.Add(new DebitNoteItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SalesTaxRate = item.SalesTaxRate,
                    LineTotal = lineTotal,
                    Remarks = item.Remarks
                });
            }

            note.TotalAmount = note.Items.Sum(i => i.LineTotal);

            _db.DebitNotes.Add(note);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Debit note {note.DebitNoteNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var note = await _db.DebitNotes.Include(d => d.Items).FirstOrDefaultAsync(d => d.Id == id);
            if (note == null) return Json(new { success = false, message = "Debit note not found." });
            if (note.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending debit notes can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            note.Status = VoucherStatus.Approved;
            note.ApprovedById = currentUser?.Id;
            note.ApprovedByName = currentUser?.FullName;
            note.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // If a return location was specified, this debit note represents goods
            // physically going back to the vendor — reverse stock in the shared ledger.
            if (note.LocationId.HasValue)
            {
                foreach (var item in note.Items)
                {
                    await _stockService.PostStockMovementAsync(
                        item.ProductId, note.LocationId.Value, -Math.Abs(item.Quantity), StockTransactionType.PurchaseReturn,
                        "DebitNote", note.Id, note.DebitNoteNumber, item.Remarks, note.DebitNoteDate);
                }
            }

            return Json(new { success = true, message = $"Debit note {note.DebitNoteNumber} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var note = await _db.DebitNotes.FindAsync(request.Id);
            if (note == null) return Json(new { success = false, message = "Debit note not found." });
            if (note.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending debit notes can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            note.Status = VoucherStatus.Rejected;
            note.ApprovedById = currentUser?.Id;
            note.ApprovedByName = currentUser?.FullName;
            note.ApprovedOn = DateTime.UtcNow;
            note.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Debit note {note.DebitNoteNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _db.DebitNotes.Include(d => d.Items).FirstOrDefaultAsync(d => d.Id == id);
            if (note == null) return Json(new { success = false, message = "Debit note not found." });

            if (note.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) debit notes can be deleted." });

            _db.DebitNoteItems.RemoveRange(note.Items);
            _db.DebitNotes.Remove(note);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Debit note deleted." });
        }

        private async Task PopulateDropdownsAsync(DebitNoteViewModel vm)
        {
            vm.Vendors = await _db.Vendors.Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .Select(v => new VendorOption(v.Id, v.Name, v.VendorCode))
                .ToListAsync();

            vm.Invoices = await _db.PurchaseInvoices.Where(i => i.Status == VoucherStatus.Approved)
                .OrderByDescending(i => i.Id)
                .Select(i => new InvoiceOption(i.Id, i.InvoiceNumber, i.TotalAmount, i.VendorId))
                .ToListAsync();

            vm.GRNs = await _db.GoodsReceiptNotes.Where(g => g.Status == VoucherStatus.Approved)
                .OrderByDescending(g => g.Id)
                .Select(g => new GRNOption(g.Id, g.GRNNumber))
                .ToListAsync();

            vm.Locations = await _db.Locations.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocationOption(l.Id, l.Name))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Purchasable | ProductType.Consumable)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new DebitNoteItemViewModel());
        }
    }
}
