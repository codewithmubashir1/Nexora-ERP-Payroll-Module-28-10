using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Purchase;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Purchase;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Purchase/Vendors")]
    public class VendorsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VendorsController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Vendors
                .Include(v => v.VendorCategory)
                .OrderByDescending(v => v.Id)
                .Select(v => new VendorListItemViewModel
                {
                    Id = v.Id,
                    VendorCode = v.VendorCode,
                    Name = v.Name,
                    CategoryName = v.VendorCategory!.Name,
                    ContactPerson = v.ContactPerson,
                    Phone = v.Phone,
                    Email = v.Email,
                    PaymentTermsDays = v.PaymentTermsDays,
                    IsActive = v.IsActive
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new VendorFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var vendor = await _db.Vendors.FindAsync(id);
            if (vendor == null) return NotFound();

            var vm = new VendorFormViewModel
            {
                Id = vendor.Id,
                VendorCode = vendor.VendorCode,
                Name = vendor.Name,
                VendorCategoryId = vendor.VendorCategoryId,
                ContactPerson = vendor.ContactPerson,
                Phone = vendor.Phone,
                Email = vendor.Email,
                Address = vendor.Address,
                City = vendor.City,
                Country = vendor.Country,
                NTNNumber = vendor.NTNNumber,
                SalesTaxRegistrationNumber = vendor.SalesTaxRegistrationNumber,
                PaymentTermsDays = vendor.PaymentTermsDays,
                OpeningBalance = vendor.OpeningBalance,
                IsActive = vendor.IsActive
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(VendorFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            Vendor vendor;

            if (isNew)
            {
                vendor = new Vendor { CreatedOn = DateTime.UtcNow, VendorCode = await GenerateVendorCodeAsync() };
                _db.Vendors.Add(vendor);
            }
            else
            {
                var existing = await _db.Vendors.FindAsync(model.Id);
                if (existing == null) return NotFound();
                vendor = existing;
            }

            vendor.Name = model.Name;
            vendor.VendorCategoryId = model.VendorCategoryId;
            vendor.ContactPerson = model.ContactPerson;
            vendor.Phone = model.Phone;
            vendor.Email = model.Email;
            vendor.Address = model.Address;
            vendor.City = model.City;
            vendor.Country = model.Country;
            vendor.NTNNumber = model.NTNNumber;
            vendor.SalesTaxRegistrationNumber = model.SalesTaxRegistrationNumber;
            vendor.PaymentTermsDays = model.PaymentTermsDays;
            vendor.OpeningBalance = model.OpeningBalance;
            vendor.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = isNew ? "Vendor created successfully." : "Vendor updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var vendor = await _db.Vendors.FindAsync(id);
            if (vendor == null) return Json(new { success = false, message = "Vendor not found." });

            var isReferenced =
                await _db.PurchaseOrders.AnyAsync(p => p.VendorId == id) ||
                await _db.GoodsReceiptNotes.AnyAsync(g => g.VendorId == id) ||
                await _db.PurchaseInvoices.AnyAsync(p => p.VendorId == id);

            if (isReferenced)
                return Json(new { success = false, message = "This vendor has purchase activity and cannot be deleted. Deactivate it instead." });

            _db.Vendors.Remove(vendor);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Vendor deleted." });
        }

        [HttpGet("Template")]
        public IActionResult Template()
        {
            var csv = CsvUtility.WriteWithHeader(
                new[] { "Name", "Category", "ContactPerson", "Phone", "Email", "Address", "City", "Country", "NTNNumber", "SalesTaxRegistrationNumber", "PaymentTermsDays", "OpeningBalance" },
                new[] { new[] { "Acme Supplies", "General", "John Doe", "+1 555 0100", "john@acme.com", "123 Main St", "Lahore", "Pakistan", "0801063-5", "02-06-8711-001-37", "30", "0" } });

            return File(csv, "text/csv", "vendor_import_template.csv");
        }

        [HttpPost("Import")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var result = new ImportResultViewModel();
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please choose a CSV file." });

            try
            {
                var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
                result.TotalRows = rows.Count;

                var categories = (await _db.VendorCategories.ToListAsync())
                    .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
                var existingByName = (await _db.Vendors.ToListAsync())
                    .GroupBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var vendorCounter = await _db.Vendors.CountAsync();
                var rowNum = 1;

                foreach (var row in rows)
                {
                    rowNum++;
                    var name = row.Get("Name");
                    var categoryName = row.Get("Category");

                    if (string.IsNullOrWhiteSpace(name)) { result.Errors.Add($"Row {rowNum}: Name is required."); result.ErrorCount++; continue; }
                    if (string.IsNullOrWhiteSpace(categoryName) || !categories.TryGetValue(categoryName, out var categoryId))
                    { result.Errors.Add($"Row {rowNum}: Category '{categoryName}' not found."); result.ErrorCount++; continue; }

                    int.TryParse(row.Get("PaymentTermsDays"), out var terms);
                    decimal.TryParse(row.Get("OpeningBalance"), out var opening);

                    Vendor vendor;
                    bool isNew;

                    if (existingByName.TryGetValue(name, out var existing))
                    {
                        vendor = existing;
                        isNew = false;
                    }
                    else
                    {
                        vendorCounter++;
                        vendor = new Vendor { CreatedOn = DateTime.UtcNow, VendorCode = $"VND-{vendorCounter:D6}" };
                        _db.Vendors.Add(vendor);
                        isNew = true;
                    }

                    vendor.Name = name;
                    vendor.VendorCategoryId = categoryId;
                    vendor.ContactPerson = row.Get("ContactPerson");
                    vendor.Phone = row.Get("Phone");
                    vendor.Email = row.Get("Email");
                    vendor.Address = row.Get("Address");
                    vendor.City = row.Get("City");
                    vendor.Country = row.Get("Country");
                    vendor.NTNNumber = row.Get("NTNNumber");
                    vendor.SalesTaxRegistrationNumber = row.Get("SalesTaxRegistrationNumber");
                    vendor.PaymentTermsDays = terms;
                    vendor.OpeningBalance = opening;
                    vendor.IsActive = true;

                    if (isNew) result.CreatedCount++; else result.UpdatedCount++;
                }

                await _db.SaveChangesAsync();
                return Json(new { success = result.Success, summary = result.Summary, errorCount = result.ErrorCount, errors = result.Errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Import failed: " + ex.Message });
            }
        }

        private async Task<string> GenerateVendorCodeAsync()
        {
            var count = await _db.Vendors.CountAsync();
            var candidate = $"VND-{(count + 1):D6}";
            while (await _db.Vendors.AnyAsync(v => v.VendorCode == candidate))
            {
                count++;
                candidate = $"VND-{(count + 1):D6}";
            }
            return candidate;
        }

        private async Task PopulateDropdownsAsync(VendorFormViewModel vm)
        {
            vm.Categories = await _db.VendorCategories.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new VendorCategoryOption(c.Id, c.Name))
                .ToListAsync();
        }
    }
}
