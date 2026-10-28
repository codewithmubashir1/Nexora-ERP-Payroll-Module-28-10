using System.Globalization;
using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Payroll/ShiftTimings")]
    public class ShiftTimingsController : Controller
    {
        private const string EditorRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;

        public ShiftTimingsController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var counts = await _db.Employees
                .Where(e => e.ShiftTimingId != null)
                .GroupBy(e => e.ShiftTimingId!.Value)
                .Select(g => new { ShiftId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ShiftId, x => x.Count);

            var shifts = await _db.ShiftTimings.OrderBy(s => s.Name).ToListAsync();

            var list = shifts.Select(s => new ShiftTimingListItemViewModel
            {
                Id = s.Id,
                Name = s.Name,
                StartTime = FormatTime(s.StartTime),
                EndTime = FormatTime(s.EndTime),
                ShiftHours = Math.Round(s.ShiftHours, 2),
                BreakMinutes = s.BreakMinutes,
                GracePeriodMinutes = s.GracePeriodMinutes,
                EmployeeCount = counts.TryGetValue(s.Id, out var c) ? c : 0,
                IsActive = s.IsActive
            }).ToList();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public IActionResult Create() => View("Form", new ShiftTimingFormViewModel());

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var shift = await _db.ShiftTimings.FindAsync(id);
            if (shift == null) return NotFound();

            return View("Form", new ShiftTimingFormViewModel
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = FormatTime(shift.StartTime),
                EndTime = FormatTime(shift.EndTime),
                BreakMinutes = shift.BreakMinutes,
                GracePeriodMinutes = shift.GracePeriodMinutes,
                IsActive = shift.IsActive
            });
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EditorRoles)]
        public async Task<IActionResult> Save(ShiftTimingFormViewModel model)
        {
            if (!TryParseTime(model.StartTime, out var start))
                ModelState.AddModelError(nameof(model.StartTime), "Enter a valid start time (HH:mm).");

            if (!TryParseTime(model.EndTime, out var end))
                ModelState.AddModelError(nameof(model.EndTime), "Enter a valid end time (HH:mm).");

            if (start == end && ModelState.IsValid)
                ModelState.AddModelError(nameof(model.EndTime), "Start and end time cannot be the same.");

            if (await _db.ShiftTimings.AnyAsync(s => s.Name == model.Name && s.Id != model.Id))
                ModelState.AddModelError(nameof(model.Name), "Another shift already uses this name.");

            if (!ModelState.IsValid) return View("Form", model);

            var isNew = model.Id == 0;
            ShiftTiming shift;

            if (isNew)
            {
                shift = new ShiftTiming { CreatedOn = DateTime.UtcNow };
                _db.ShiftTimings.Add(shift);
            }
            else
            {
                var existing = await _db.ShiftTimings.FindAsync(model.Id);
                if (existing == null) return NotFound();
                shift = existing;
            }

            shift.Name = model.Name;
            shift.StartTime = start;
            shift.EndTime = end;
            shift.BreakMinutes = model.BreakMinutes;
            shift.GracePeriodMinutes = model.GracePeriodMinutes;
            shift.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = isNew ? "Shift created successfully." : "Shift updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _db.ShiftTimings.FindAsync(id);
            if (shift == null) return Json(new { success = false, message = "Shift not found." });

            if (await _db.Employees.AnyAsync(e => e.ShiftTimingId == id))
                return Json(new { success = false, message = "Employees are assigned to this shift. Reassign them first, or deactivate the shift." });

            _db.ShiftTimings.Remove(shift);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Shift deleted." });
        }

        private static string FormatTime(TimeSpan value) => value.ToString(@"hh\:mm");

        private static bool TryParseTime(string? value, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(value)) return false;

            // <input type="time"> posts HH:mm, but HH:mm:ss shows up in some browsers.
            return TimeSpan.TryParseExact(value, new[] { @"hh\:mm", @"h\:mm", @"hh\:mm\:ss" },
                CultureInfo.InvariantCulture, out result);
        }
    }
}
