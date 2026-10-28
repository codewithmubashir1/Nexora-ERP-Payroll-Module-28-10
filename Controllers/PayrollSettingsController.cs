using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>
    /// One sectioned screen for every piece of payroll configuration: reimbursement and
    /// leave types, holidays, statutory deductions, tax slabs, the four attendance
    /// policies, company-wide settings, devices and machine integration.
    /// Each section posts its own small form and returns to that section.
    /// </summary>
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}")]
    [Route("Payroll/Settings")]
    public class PayrollSettingsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;

        public PayrollSettingsController(ApplicationDbContext db, IPayrollService payrollService)
        {
            _db = db;
            _payrollService = payrollService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? tab, int? holidayYear)
        {
            var year = holidayYear ?? DateTime.Today.Year;

            var vm = new PayrollSettingsViewModel
            {
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "reimbursement" : tab,
                HolidayYear = year,
                ReimbursementTypes = await _db.ReimbursementTypes.OrderBy(r => r.Name).ToListAsync(),
                LeaveTypes = await _db.LeaveTypes.OrderBy(l => l.Name).ToListAsync(),
                Holidays = await _db.Holidays.Where(h => h.Date.Year == year).OrderBy(h => h.Date).ToListAsync(),
                StatutoryDeductions = await _db.StatutoryDeductions.OrderBy(s => s.Name).ToListAsync(),
                TaxSlabs = await _db.TaxSlabs.OrderBy(t => t.MinIncome).ToListAsync(),
                Policies = await _db.AttendancePolicies.OrderBy(p => p.PolicyType).ToListAsync(),
                Devices = await _db.AttendanceDevices.OrderBy(d => d.Name).ToListAsync(),
                Other = await _payrollService.GetSettingsAsync(),
                MachineIntegration = await _payrollService.GetMachineIntegrationAsync()
            };

            // The four policy rows are seeded, but a database restored from an older
            // snapshot might be missing one — fill any gap so the tab always renders.
            foreach (AttendancePolicyType type in Enum.GetValues<AttendancePolicyType>())
            {
                if (vm.Policies.All(p => p.PolicyType != type))
                {
                    var policy = new AttendancePolicy { PolicyType = type };
                    _db.AttendancePolicies.Add(policy);
                    vm.Policies.Add(policy);
                }
            }

            if (_db.ChangeTracker.HasChanges())
            {
                await _db.SaveChangesAsync();
                vm.Policies = vm.Policies.OrderBy(p => p.PolicyType).ToList();
            }

            return View(vm);
        }

        // ---------------- Reimbursement types ----------------

        [HttpPost("SaveReimbursementType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReimbursementType(int id, string name, decimal? maxLimit, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "A name is required.";
                return Back("reimbursement");
            }

            var entity = id == 0 ? new ReimbursementType { CreatedOn = DateTime.UtcNow } : await _db.ReimbursementTypes.FindAsync(id);
            if (entity == null) return NotFound();
            if (id == 0) _db.ReimbursementTypes.Add(entity);

            entity.Name = name.Trim();
            entity.MaxLimit = maxLimit;
            entity.IsActive = isActive;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Expense type saved.";
            return Back("reimbursement");
        }

        [HttpPost("DeleteReimbursementType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReimbursementType(int id)
        {
            var entity = await _db.ReimbursementTypes.FindAsync(id);
            if (entity == null) return Json(new { success = false, message = "Expense type not found." });

            if (await _db.ReimbursementRequests.AnyAsync(r => r.ReimbursementTypeId == id))
                return Json(new { success = false, message = "Requests already use this expense type. Deactivate it instead." });

            _db.ReimbursementTypes.Remove(entity);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Expense type deleted." });
        }

        // ---------------- Leave types ----------------

        [HttpPost("SaveLeaveType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLeaveType(int id, string name, int daysPerYear, bool isPaid, bool isEncashable, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "A name is required.";
                return Back("leaves");
            }

            var entity = id == 0 ? new LeaveType { CreatedOn = DateTime.UtcNow } : await _db.LeaveTypes.FindAsync(id);
            if (entity == null) return NotFound();
            if (id == 0) _db.LeaveTypes.Add(entity);

            entity.Name = name.Trim();
            entity.DaysPerYear = Math.Max(0, daysPerYear);
            entity.IsPaid = isPaid;
            entity.IsEncashable = isEncashable;
            entity.IsActive = isActive;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Leave type saved.";
            return Back("leaves");
        }

        [HttpPost("DeleteLeaveType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLeaveType(int id)
        {
            var entity = await _db.LeaveTypes.FindAsync(id);
            if (entity == null) return Json(new { success = false, message = "Leave type not found." });

            if (await _db.LeaveRequests.AnyAsync(l => l.LeaveTypeId == id) ||
                await _db.LeaveEncashmentRequests.AnyAsync(l => l.LeaveTypeId == id))
                return Json(new { success = false, message = "Requests already use this leave type. Deactivate it instead." });

            _db.LeaveTypes.Remove(entity);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Leave type deleted." });
        }

        // ---------------- Holidays ----------------

        [HttpPost("SaveHoliday")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHoliday(int id, DateTime date, string name, bool isOptional)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "A holiday name is required.";
                return Back("holidays", date.Year);
            }

            if (await _db.Holidays.AnyAsync(h => h.Date == date.Date && h.Id != id))
            {
                TempData["ErrorMessage"] = $"A holiday is already recorded for {date:dd MMM yyyy}.";
                return Back("holidays", date.Year);
            }

            var entity = id == 0 ? new Holiday { CreatedOn = DateTime.UtcNow } : await _db.Holidays.FindAsync(id);
            if (entity == null) return NotFound();
            if (id == 0) _db.Holidays.Add(entity);

            entity.Date = date.Date;
            entity.Name = name.Trim();
            entity.IsOptional = isOptional;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Holiday saved.";
            return Back("holidays", date.Year);
        }

        [HttpPost("DeleteHoliday")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var entity = await _db.Holidays.FindAsync(id);
            if (entity == null) return Json(new { success = false, message = "Holiday not found." });

            _db.Holidays.Remove(entity);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Holiday deleted." });
        }

        // ---------------- Statutory deductions ----------------

        [HttpPost("SaveStatutoryDeduction")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStatutoryDeduction(int id, string name, decimal ratePercent, decimal? maxApplicableSalary, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "A name is required.";
                return Back("statutory");
            }

            if (ratePercent < 0 || ratePercent > 100)
            {
                TempData["ErrorMessage"] = "The rate must be between 0 and 100 percent.";
                return Back("statutory");
            }

            var entity = id == 0 ? new StatutoryDeduction { CreatedOn = DateTime.UtcNow } : await _db.StatutoryDeductions.FindAsync(id);
            if (entity == null) return NotFound();
            if (id == 0) _db.StatutoryDeductions.Add(entity);

            entity.Name = name.Trim();
            entity.RatePercent = ratePercent;
            entity.MaxApplicableSalary = maxApplicableSalary;
            entity.IsActive = isActive;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Statutory deduction saved.";
            return Back("statutory");
        }

        [HttpPost("DeleteStatutoryDeduction")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStatutoryDeduction(int id)
        {
            var entity = await _db.StatutoryDeductions.FindAsync(id);
            if (entity == null) return Json(new { success = false, message = "Deduction not found." });

            _db.StatutoryDeductions.Remove(entity);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Statutory deduction deleted." });
        }

        // ---------------- Tax slabs ----------------

        [HttpPost("SaveTaxSlab")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTaxSlab(int id, decimal minIncome, decimal? maxIncome, decimal ratePercent, decimal fixedAmount, bool isActive)
        {
            if (maxIncome.HasValue && maxIncome.Value <= minIncome)
            {
                TempData["ErrorMessage"] = "The upper bound must be greater than the lower bound.";
                return Back("statutory");
            }

            var entity = id == 0 ? new TaxSlab { CreatedOn = DateTime.UtcNow } : await _db.TaxSlabs.FindAsync(id);
            if (entity == null) return NotFound();
            if (id == 0) _db.TaxSlabs.Add(entity);

            entity.MinIncome = minIncome;
            entity.MaxIncome = maxIncome;
            entity.RatePercent = ratePercent;
            entity.FixedAmount = fixedAmount;
            entity.IsActive = isActive;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tax slab saved.";
            return Back("statutory");
        }

        [HttpPost("DeleteTaxSlab")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTaxSlab(int id)
        {
            var entity = await _db.TaxSlabs.FindAsync(id);
            if (entity == null) return Json(new { success = false, message = "Tax slab not found." });

            _db.TaxSlabs.Remove(entity);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Tax slab deleted." });
        }

        // ---------------- Policies and rules ----------------

        [HttpPost("SavePolicy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePolicy(int id, int gracePeriodMinutes, int maxOccurrencesPerMonth,
            PenaltyType penaltyOnExceeding, string? compensationType, decimal? overtimeRateMultiplier, string? notes)
        {
            var policy = await _db.AttendancePolicies.FindAsync(id);
            if (policy == null) return NotFound();

            policy.GracePeriodMinutes = Math.Max(0, gracePeriodMinutes);
            policy.MaxOccurrencesPerMonth = Math.Max(0, maxOccurrencesPerMonth);
            policy.PenaltyOnExceeding = penaltyOnExceeding;
            policy.CompensationType = policy.PolicyType == AttendancePolicyType.HolidayWorking ? compensationType : null;
            policy.OvertimeRateMultiplier = overtimeRateMultiplier;
            policy.Notes = notes;
            policy.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{policy.PolicyType} rule saved.";
            return Back("policies");
        }

        // ---------------- Other settings ----------------

        [HttpPost("SaveOtherSettings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOtherSettings(OtherPayrollSettings model)
        {
            var settings = await _payrollService.GetSettingsAsync();

            settings.WorkingDaysPerWeek = Math.Clamp(model.WorkingDaysPerWeek, 1, 7);
            settings.FiscalYearStartMonth = Math.Clamp(model.FiscalYearStartMonth, 1, 12);
            settings.DefaultCurrency = string.IsNullOrWhiteSpace(model.DefaultCurrency) ? "PKR" : model.DefaultCurrency.Trim();
            settings.OvertimeRateMultiplier = model.OvertimeRateMultiplier;
            settings.RoundOffNetPay = model.RoundOffNetPay;
            settings.PayrollCutoffDay = Math.Clamp(model.PayrollCutoffDay, 1, 31);
            settings.MonthlyFixedDeduction = Math.Max(0, model.MonthlyFixedDeduction);
            settings.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Payroll settings saved.";
            return Back("other");
        }

        // ---------------- Attendance devices ----------------

        [HttpPost("SaveDevice")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDevice(int id, string name, string? location, AttendanceDeviceType deviceType,
            string? ipAddress, string? serialNumber, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "A device name is required.";
                return Back("devices");
            }

            var entity = id == 0 ? new AttendanceDevice { CreatedOn = DateTime.UtcNow } : await _db.AttendanceDevices.FindAsync(id);
            if (entity == null) return NotFound();
            if (id == 0) _db.AttendanceDevices.Add(entity);

            entity.Name = name.Trim();
            entity.Location = location;
            entity.DeviceType = deviceType;
            entity.IPAddress = ipAddress;
            entity.SerialNumber = serialNumber;
            entity.IsActive = isActive;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Device saved.";
            return Back("devices");
        }

        [HttpPost("DeleteDevice")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var entity = await _db.AttendanceDevices.FindAsync(id);
            if (entity == null) return Json(new { success = false, message = "Device not found." });

            _db.AttendanceDevices.Remove(entity);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Device deleted." });
        }

        // ---------------- Machine integration ----------------

        [HttpPost("SaveMachineIntegration")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMachineIntegration(MachineIntegrationSetting model)
        {
            var settings = await _payrollService.GetMachineIntegrationAsync();

            settings.SyncMode = model.SyncMode == "Scheduled" ? "Scheduled" : "Manual";
            settings.SyncIntervalMinutes = Math.Clamp(model.SyncIntervalMinutes, 1, 1440);
            settings.ApiEndpoint = model.ApiEndpoint;
            settings.ApiKey = model.ApiKey;
            settings.Notes = model.Notes;
            settings.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Machine integration settings saved.";
            return Back("machine");
        }

        // ---------------- Increments ----------------

        [HttpGet("Increments")]
        public async Task<IActionResult> Increments(int? id)
        {
            var vm = new SalaryIncrementFormViewModel
            {
                Employees = await _db.Employees.Where(e => e.IsActive)
                    .OrderBy(e => e.Name)
                    .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                    .ToListAsync()
            };

            if (id.HasValue)
            {
                var increment = await _db.SalaryIncrements.FindAsync(id.Value);
                if (increment == null) return NotFound();
                vm.Id = increment.Id;
                vm.EmployeeId = increment.EmployeeId;
                vm.EffectiveDate = increment.EffectiveDate;
                vm.OldBasicSalary = increment.OldBasicSalary;
                vm.NewBasicSalary = increment.NewBasicSalary;
                vm.Reason = increment.Reason;
            }

            return View(vm);
        }

        [HttpGet("GetIncrements")]
        public async Task<IActionResult> GetIncrements()
        {
            var increments = await _db.SalaryIncrements
                .Include(s => s.Employee)
                .OrderByDescending(s => s.EffectiveDate).ThenByDescending(s => s.Id)
                .ToListAsync();

            var list = increments.Select(s => new SalaryIncrementListItemViewModel
            {
                Id = s.Id,
                EmployeeName = s.Employee?.Name ?? "—",
                EmployeeCode = s.Employee?.EmployeeCode ?? "—",
                EffectiveDate = s.EffectiveDate,
                OldBasicSalary = s.OldBasicSalary,
                NewBasicSalary = s.NewBasicSalary,
                IncreaseAmount = s.NewBasicSalary - s.OldBasicSalary,
                IncreasePercent = s.OldBasicSalary > 0
                    ? Math.Round((s.NewBasicSalary - s.OldBasicSalary) / s.OldBasicSalary * 100m, 2)
                    : 0m,
                Reason = s.Reason,
                ApprovedByName = s.ApprovedByName
            }).ToList();

            return Json(new { data = list });
        }

        [HttpPost("SaveIncrement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveIncrement(SalaryIncrementFormViewModel model)
        {
            var existingIncrement = model.Id == 0 ? null : await _db.SalaryIncrements.FindAsync(model.Id);
            if (model.Id != 0 && existingIncrement == null) return NotFound();

            var employee = await _db.Employees.FindAsync(model.EmployeeId);
            if (employee == null)
                ModelState.AddModelError(nameof(model.EmployeeId), "Select a valid employee.");
            else
            {
                if (existingIncrement != null && existingIncrement.EmployeeId != model.EmployeeId)
                    ModelState.AddModelError(nameof(model.EmployeeId), "An increment cannot be reassigned to another employee.");
                var baseSalary = existingIncrement?.OldBasicSalary ?? employee.BasicSalary;
                if (model.NewBasicSalary == baseSalary)
                    ModelState.AddModelError(nameof(model.NewBasicSalary), "The new basic salary must differ from the previous basic salary.");
            }

            if (!ModelState.IsValid)
            {
                model.Employees = await _db.Employees.Where(e => e.IsActive)
                    .OrderBy(e => e.Name)
                    .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                    .ToListAsync();
                return View("Increments", model);
            }

            var previousEffectiveDate = existingIncrement?.EffectiveDate;
            var oldBasicSalary = existingIncrement?.OldBasicSalary ?? employee!.BasicSalary;
            var employeeSalaryMatchesIncrement = existingIncrement != null &&
                existingIncrement.EffectiveDate <= DateTime.Today && employee!.BasicSalary == existingIncrement.NewBasicSalary;

            var increment = existingIncrement ?? new SalaryIncrement
            {
                CreatedOn = DateTime.UtcNow,
                ApprovedByName = User.Identity?.Name
            };
            increment.EmployeeId = model.EmployeeId;
            increment.EffectiveDate = model.EffectiveDate.Date;
            increment.OldBasicSalary = oldBasicSalary;
            increment.NewBasicSalary = model.NewBasicSalary;
            increment.Reason = model.Reason;
            if (existingIncrement == null) _db.SalaryIncrements.Add(increment);

            var wasFuture = previousEffectiveDate.HasValue && previousEffectiveDate.Value > DateTime.Today;
            var movedToFuture = increment.EffectiveDate > DateTime.Today;
            if ((existingIncrement == null && increment.EffectiveDate <= DateTime.Today) ||
                (employeeSalaryMatchesIncrement && !movedToFuture) ||
                (wasFuture && increment.EffectiveDate <= DateTime.Today))
            {
                employee!.BasicSalary = model.NewBasicSalary;
                employee.MonthlySalary = employee.BasicSalary + employee.HouseRentAllowance
                    + employee.MedicalAllowance + employee.OtherAllowances;
            }
            else if (employeeSalaryMatchesIncrement && movedToFuture)
            {
                employee!.BasicSalary = oldBasicSalary;
                employee.MonthlySalary = employee.BasicSalary + employee.HouseRentAllowance
                    + employee.MedicalAllowance + employee.OtherAllowances;
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = increment.EffectiveDate <= DateTime.Today
                ? $"Increment {(existingIncrement == null ? "applied" : "updated")} — {employee!.Name} now has a basic salary of {model.NewBasicSalary:N2}."
                : $"Increment {(existingIncrement == null ? "recorded" : "updated")} for {increment.EffectiveDate:dd MMM yyyy}. Apply it on that date.";

            return RedirectToAction(nameof(Increments));
        }

        [HttpPost("DeleteIncrement")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> DeleteIncrement(int id)
        {
            var increment = await _db.SalaryIncrements.FindAsync(id);
            if (increment == null) return Json(new { success = false, message = "Increment not found." });

            var employee = await _db.Employees.FindAsync(increment.EmployeeId);
            var hasLaterAppliedIncrement = await _db.SalaryIncrements.AnyAsync(other =>
                other.EmployeeId == increment.EmployeeId && other.Id != increment.Id && other.EffectiveDate <= DateTime.Today &&
                (other.EffectiveDate > increment.EffectiveDate || (other.EffectiveDate == increment.EffectiveDate && other.Id > increment.Id)));
            var revertEmployeeSalary = employee != null && !hasLaterAppliedIncrement &&
                increment.EffectiveDate <= DateTime.Today && employee.BasicSalary == increment.NewBasicSalary;

            _db.SalaryIncrements.Remove(increment);
            if (revertEmployeeSalary)
            {
                employee!.BasicSalary = increment.OldBasicSalary;
                employee.MonthlySalary = employee.BasicSalary + employee.HouseRentAllowance
                    + employee.MedicalAllowance + employee.OtherAllowances;
            }
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = revertEmployeeSalary ? "Increment removed and the previous basic salary restored." : "Increment record removed." });
        }

        private IActionResult Back(string tab, int? holidayYear = null) =>
            RedirectToAction(nameof(Index), new { tab, holidayYear });
    }
}
