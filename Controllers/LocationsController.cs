using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Inventory/Locations")]
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LocationsController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Locations
                .OrderBy(l => l.Name)
                .Select(l => new LocationViewModel
                {
                    Id = l.Id,
                    Name = l.Name,
                    Code = l.Code,
                    Address = l.Address,
                    IsActive = l.IsActive
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public IActionResult Create() => View("Form", new LocationViewModel());

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var location = await _db.Locations.FindAsync(id);
            if (location == null) return NotFound();

            return View("Form", new LocationViewModel
            {
                Id = location.Id,
                Name = location.Name,
                Code = location.Code,
                Address = location.Address,
                IsActive = location.IsActive
            });
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.StoreIncharge},{AppRoles.Manager}")]
        public async Task<IActionResult> Save(LocationViewModel model)
        {
            if (!ModelState.IsValid) return View("Form", model);

            var duplicateExists = await _db.Locations.AnyAsync(l => l.Name == model.Name && l.Id != model.Id);
            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(model.Name), "A location with this name already exists.");
                return View("Form", model);
            }

            if (model.Id == 0)
            {
                _db.Locations.Add(new Location
                {
                    Name = model.Name,
                    Code = model.Code,
                    Address = model.Address,
                    IsActive = model.IsActive
                });
                TempData["SuccessMessage"] = "Location created successfully.";
            }
            else
            {
                var location = await _db.Locations.FindAsync(model.Id);
                if (location == null) return NotFound();

                location.Name = model.Name;
                location.Code = model.Code;
                location.Address = model.Address;
                location.IsActive = model.IsActive;
                TempData["SuccessMessage"] = "Location updated successfully.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _db.Locations.FindAsync(id);
            if (location == null) return Json(new { success = false, message = "Location not found." });

            var isReferenced =
                await _db.Products.AnyAsync(p => p.DefaultLocationId == id) ||
                await _db.StockTransactions.AnyAsync(s => s.LocationId == id) ||
                await _db.InternalTransfers.AnyAsync(t => t.FromLocationId == id || t.ToLocationId == id) ||
                await _db.ItemAdjustmentNotes.AnyAsync(n => n.LocationId == id) ||
                await _db.ConsumptionVouchers.AnyAsync(v => v.LocationId == id);

            if (isReferenced)
                return Json(new { success = false, message = "This location has stock activity and cannot be deleted. Deactivate it instead." });

            _db.Locations.Remove(location);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Location deleted." });
        }
        [HttpGet("Template")]
        public IActionResult Template()
        {
            var csv = CsvUtility.WriteWithHeader(
                new[] { "Name", "Code", "Address" },
                new[] { new[] { "Downtown Store", "ST-DT", "Sample row — delete before importing" } });

            return File(csv, "text/csv", "location_import_template.csv");
        }

        [HttpPost("Import")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.StoreIncharge},{AppRoles.Manager}")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var result = new ImportResultViewModel();
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please choose a CSV file." });

            try
            {
                var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
                result.TotalRows = rows.Count;

                var existing = (await _db.Locations.ToListAsync())
                    .GroupBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var rowNum = 1;
                foreach (var row in rows)
                {
                    rowNum++;
                    var name = row.Get("Name");
                    if (string.IsNullOrWhiteSpace(name)) { result.Errors.Add($"Row {rowNum}: Name is required."); result.ErrorCount++; continue; }

                    if (existing.TryGetValue(name, out var location))
                    {
                        location.Code = row.Get("Code");
                        location.Address = row.Get("Address");
                        result.UpdatedCount++;
                    }
                    else
                    {
                        _db.Locations.Add(new Location
                        {
                            Name = name,
                            Code = row.Get("Code"),
                            Address = row.Get("Address"),
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
