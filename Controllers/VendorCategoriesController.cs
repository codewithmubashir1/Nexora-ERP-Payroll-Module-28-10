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
    [Route("Purchase/VendorCategories")]
    public class VendorCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VendorCategoriesController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.VendorCategories
                .OrderBy(c => c.Name)
                .Select(c => new VendorCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    VendorCount = c.Vendors.Count
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public IActionResult Create() => View("Form", new VendorCategoryViewModel());

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.VendorCategories.FindAsync(id);
            if (category == null) return NotFound();

            return View("Form", new VendorCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Description = category.Description,
                IsActive = category.IsActive
            });
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        public async Task<IActionResult> Save(VendorCategoryViewModel model)
        {
            if (!ModelState.IsValid) return View("Form", model);

            var duplicateExists = await _db.VendorCategories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id);
            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View("Form", model);
            }

            if (model.Id == 0)
            {
                _db.VendorCategories.Add(new VendorCategory
                {
                    Name = model.Name,
                    Code = model.Code,
                    Description = model.Description,
                    IsActive = model.IsActive
                });
                TempData["SuccessMessage"] = "Vendor category created successfully.";
            }
            else
            {
                var category = await _db.VendorCategories.FindAsync(model.Id);
                if (category == null) return NotFound();

                category.Name = model.Name;
                category.Code = model.Code;
                category.Description = model.Description;
                category.IsActive = model.IsActive;
                TempData["SuccessMessage"] = "Vendor category updated successfully.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.VendorCategories.Include(c => c.Vendors).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return Json(new { success = false, message = "Category not found." });

            if (category.Vendors.Any())
                return Json(new { success = false, message = "This category is used by one or more vendors and cannot be deleted." });

            _db.VendorCategories.Remove(category);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Category deleted." });
        }

        [HttpGet("Template")]
        public IActionResult Template()
        {
            var csv = CsvUtility.WriteWithHeader(
                new[] { "Name", "Code", "Description" },
                new[] { new[] { "Equipment Supplier", "EQP", "Sample row — delete before importing" } });

            return File(csv, "text/csv", "vendor_category_import_template.csv");
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

                var existing = (await _db.VendorCategories.ToListAsync())
                    .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var rowNum = 1;
                foreach (var row in rows)
                {
                    rowNum++;
                    var name = row.Get("Name");
                    if (string.IsNullOrWhiteSpace(name)) { result.Errors.Add($"Row {rowNum}: Name is required."); result.ErrorCount++; continue; }

                    if (existing.TryGetValue(name, out var category))
                    {
                        category.Code = row.Get("Code");
                        category.Description = row.Get("Description");
                        result.UpdatedCount++;
                    }
                    else
                    {
                        _db.VendorCategories.Add(new VendorCategory
                        {
                            Name = name,
                            Code = row.Get("Code"),
                            Description = row.Get("Description"),
                            IsActive = true
                        });
                        result.CreatedCount++;
                    }
                }

                await _db.SaveChangesAsync();
                return Json(new { success = result.Success, summary = result.Summary, errorCount = result.ErrorCount, errors = result.Errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Import failed: " + ex.Message });
            }
        }
    }
}
