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
    [Route("Payroll/Departments")]
    public class DepartmentsController : Controller
    {
        private const string EditorRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;

        public DepartmentsController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var employeeCounts = await _db.Employees
                .Where(e => e.DepartmentId != null)
                .GroupBy(e => e.DepartmentId!.Value)
                .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

            var departments = await _db.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.HeadEmployee)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var list = departments.Select(d => new DepartmentListItemViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                ParentName = d.ParentDepartment?.Name,
                HeadName = d.HeadEmployee?.Name,
                EmployeeCount = employeeCounts.TryGetValue(d.Id, out var c) ? c : 0,
                IsActive = d.IsActive
            }).ToList();

            return Json(new { data = list });
        }

        /// <summary>Renders the parent/child department tree.</summary>
        [HttpGet("Hierarchy")]
        public async Task<IActionResult> Hierarchy()
        {
            var departments = await _db.Departments
                .Include(d => d.HeadEmployee)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var employeeCounts = await _db.Employees
                .Where(e => e.DepartmentId != null)
                .GroupBy(e => e.DepartmentId!.Value)
                .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

            var nodes = departments.ToDictionary(d => d.Id, d => new DepartmentNodeViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                HeadName = d.HeadEmployee?.Name,
                EmployeeCount = employeeCounts.TryGetValue(d.Id, out var c) ? c : 0,
                IsActive = d.IsActive
            });

            var roots = new List<DepartmentNodeViewModel>();

            foreach (var department in departments)
            {
                var node = nodes[department.Id];
                if (department.ParentDepartmentId.HasValue && nodes.TryGetValue(department.ParentDepartmentId.Value, out var parent))
                    parent.Children.Add(node);
                else
                    roots.Add(node);
            }

            return View(roots);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new DepartmentFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            if (department == null) return NotFound();

            var vm = new DepartmentFormViewModel
            {
                Id = department.Id,
                Name = department.Name,
                Code = department.Code,
                ParentDepartmentId = department.ParentDepartmentId,
                HeadEmployeeId = department.HeadEmployeeId,
                Description = department.Description,
                IsActive = department.IsActive
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EditorRoles)]
        public async Task<IActionResult> Save(DepartmentFormViewModel model)
        {
            if (model.ParentDepartmentId == model.Id && model.Id != 0)
                ModelState.AddModelError(nameof(model.ParentDepartmentId), "A department cannot be its own parent.");

            if (model.Id != 0 && model.ParentDepartmentId.HasValue &&
                await WouldCreateCycleAsync(model.Id, model.ParentDepartmentId.Value))
                ModelState.AddModelError(nameof(model.ParentDepartmentId), "That parent sits under this department — it would create a loop.");

            if (!string.IsNullOrWhiteSpace(model.Code) &&
                await _db.Departments.AnyAsync(d => d.Code == model.Code && d.Id != model.Id))
                ModelState.AddModelError(nameof(model.Code), "Another department already uses this code.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            Department department;

            if (isNew)
            {
                department = new Department { CreatedOn = DateTime.UtcNow };
                _db.Departments.Add(department);
            }
            else
            {
                var existing = await _db.Departments.FindAsync(model.Id);
                if (existing == null) return NotFound();
                department = existing;
            }

            department.Name = model.Name;
            department.Code = model.Code;
            department.ParentDepartmentId = model.ParentDepartmentId;
            department.HeadEmployeeId = model.HeadEmployeeId;
            department.Description = model.Description;
            department.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = isNew ? "Department created successfully." : "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            if (department == null) return Json(new { success = false, message = "Department not found." });

            if (await _db.Departments.AnyAsync(d => d.ParentDepartmentId == id))
                return Json(new { success = false, message = "This department has sub-departments. Move or remove them first." });

            if (await _db.Employees.AnyAsync(e => e.DepartmentId == id))
                return Json(new { success = false, message = "Employees are assigned to this department. Reassign them first, or deactivate the department." });

            _db.Departments.Remove(department);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Department deleted." });
        }

        /// <summary>Walks up from the proposed parent; if we meet ourselves, the move would create a loop.</summary>
        private async Task<bool> WouldCreateCycleAsync(int departmentId, int proposedParentId)
        {
            var all = await _db.Departments
                .Select(d => new { d.Id, d.ParentDepartmentId })
                .ToListAsync();

            var lookup = all.ToDictionary(d => d.Id, d => d.ParentDepartmentId);
            var cursor = (int?)proposedParentId;
            var guard = 0;

            while (cursor.HasValue && guard++ < 100)
            {
                if (cursor.Value == departmentId) return true;
                cursor = lookup.TryGetValue(cursor.Value, out var parent) ? parent : null;
            }

            return false;
        }

        private async Task PopulateDropdownsAsync(DepartmentFormViewModel vm)
        {
            vm.ParentOptions = await _db.Departments
                .Where(d => d.IsActive && d.Id != vm.Id)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentOption(d.Id, d.Name, d.Code))
                .ToListAsync();

            vm.Employees = await _db.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                .ToListAsync();
        }
    }
}
