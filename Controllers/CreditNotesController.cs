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
    [Route("Sales/CreditNotes")]
    public class CreditNotesController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyInfoService _companyInfoService;

        public CreditNotesController(ApplicationDbContext db, IStockService stockService, UserManager<ApplicationUser> userManager, ICompanyInfoService companyInfoService)
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
            var list = await _db.CreditNotes
                .Include(c => c.Customer)
                .Include(c => c.Items)
                .OrderByDescending(c => c.Id)
                .Select(c => new CreditNoteListItemViewModel
                {
                    Id = c.Id,
                    CreditNoteNumber = c.CreditNoteNumber,
                    CreditNoteDate = c.CreditNoteDate,
                    CustomerName = c.Customer!.Name,
                    TotalAmount = c.TotalAmount,
                    Reason = c.Reason,
                    ItemCount = c.Items.Count,
                    Status = c.Status.ToString()
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new CreditNoteViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var note = await _db.CreditNotes
                .Include(c => c.Customer)
                .Include(c => c.SalesInvoice)
                .Include(c => c.DeliveryChallan)
                .Include(c => c.Location)
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (note == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View(note);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var note = await _db.CreditNotes
                .Include(c => c.Customer)
                .Include(c => c.SalesInvoice)
                .Include(c => c.DeliveryChallan)
                .Include(c => c.Location)
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (note == null) return NotFound();

            var taxTotal = note.Items.Sum(i => i.LineTotal - (i.Quantity * i.UnitPrice));

            var vm = new PrintDocumentViewModel
            {
                DocumentTypeTitle = "CREDIT NOTE",
                DocumentNumberLabel = "CREDIT NOTE #",
                DocumentNumber = note.CreditNoteNumber,
                Status = note.Status.ToString(),
                ExtraRefs = new()
                {
                    ("DATE", note.CreditNoteDate.ToString("dd-MM-yyyy")),
                    ("REASON", note.Reason ?? "—"),
                    ("INVOICE #", note.SalesInvoice?.InvoiceNumber ?? "—"),
                    ("DELIVERY CHALLAN #", note.DeliveryChallan?.ChallanNo ?? "—"),
                    ("RETURN LOCATION", note.Location?.Name ?? "Financial claim only")
                },
                PartyLabel = "CUSTOMER",
                PartyCode = note.Customer?.CustomerCode,
                PartyVendorCode = note.Customer?.OurVendorCodeAtCustomer,
                PartyName = note.Customer?.Name,
                PartyAddress = note.Customer?.Address,
                PartyEmail = note.Customer?.Email,
                PartyPhone = note.Customer?.Phone,
                PartyNTN = note.Customer?.NTNNumber,
                PartySTRN = note.Customer?.SalesTaxRegistrationNumber,
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

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApproverRoles},{AppRoles.StandardUser}")]
        public async Task<IActionResult> Save(CreditNoteViewModel model)
        {
            model.Items = (model.Items ?? new()).Where(i => i.ProductId != 0).ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product line.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var note = new CreditNote
            {
                CreditNoteNumber = await _stockService.GenerateDocumentNumberAsync("CN", _db.CreditNotes.Select(c => c.CreditNoteNumber)),
                CreditNoteDate = model.CreditNoteDate,
                CustomerId = model.CustomerId,
                SalesInvoiceId = model.SalesInvoiceId,
                DeliveryChallanId = model.DeliveryChallanId,
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

                note.Items.Add(new CreditNoteItem
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

            _db.CreditNotes.Add(note);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Credit note {note.CreditNoteNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var note = await _db.CreditNotes.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
            if (note == null) return Json(new { success = false, message = "Credit note not found." });
            if (note.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending credit notes can be approved." });

            var currentUser = await _userManager.GetUserAsync(User);
            note.Status = VoucherStatus.Approved;
            note.ApprovedById = currentUser?.Id;
            note.ApprovedByName = currentUser?.FullName;
            note.ApprovedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // If a return location was specified, goods physically came back from the
            // customer — reverse stock in on the shared ledger.
            if (note.LocationId.HasValue)
            {
                foreach (var item in note.Items)
                {
                    await _stockService.PostStockMovementAsync(
                        item.ProductId, note.LocationId.Value, Math.Abs(item.Quantity), StockTransactionType.SalesReturn,
                        "CreditNote", note.Id, note.CreditNoteNumber, item.Remarks, note.CreditNoteDate);
                }
            }

            return Json(new { success = true, message = $"Credit note {note.CreditNoteNumber} approved." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var note = await _db.CreditNotes.FindAsync(request.Id);
            if (note == null) return Json(new { success = false, message = "Credit note not found." });
            if (note.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending credit notes can be rejected." });

            var currentUser = await _userManager.GetUserAsync(User);
            note.Status = VoucherStatus.Rejected;
            note.ApprovedById = currentUser?.Id;
            note.ApprovedByName = currentUser?.FullName;
            note.ApprovedOn = DateTime.UtcNow;
            note.RejectionReason = request.Reason;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Credit note {note.CreditNoteNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _db.CreditNotes.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
            if (note == null) return Json(new { success = false, message = "Credit note not found." });

            if (note.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending (not yet approved) credit notes can be deleted." });

            _db.CreditNoteItems.RemoveRange(note.Items);
            _db.CreditNotes.Remove(note);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Credit note deleted." });
        }

        private async Task PopulateDropdownsAsync(CreditNoteViewModel vm)
        {
            vm.Customers = await _db.Customers.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CustomerOption(c.Id, c.Name, c.CustomerCode))
                .ToListAsync();

            var approvedInvoices = await _db.SalesInvoices.Where(i => i.Status == VoucherStatus.Approved).ToListAsync();
            vm.Invoices = approvedInvoices
                .Select(i => new SalesInvoiceOption(i.Id, i.InvoiceNumber, i.TotalAmount, i.CustomerId))
                .ToList();

            vm.DeliveryChallans = await _db.DeliveryChallans.Where(d => d.Status == VoucherStatus.Approved)
                .OrderByDescending(d => d.Id)
                .Select(d => new DeliveryChallanOption(d.Id, d.ChallanNo))
                .ToListAsync();

            vm.Locations = await _db.Locations.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocationOption(l.Id, l.Name))
                .ToListAsync();

            vm.Products = await _db.Products.Where(p => p.IsActive && (p.ProductType & (ProductType.Saleable | ProductType.Service)) != 0)
                .OrderBy(p => p.Name)
                .Select(p => new ProductOption(p.Id, p.Name, p.Sku, p.CostPrice, p.SalePrice, p.PackingUnitName, p.PackingQuantity, p.ProductCode))
                .ToListAsync();

            if (vm.Items.Count == 0) vm.Items.Add(new CreditNoteItemViewModel());
        }
    }
}
