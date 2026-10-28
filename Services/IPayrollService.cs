using ERP.Identity.Data;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Payroll;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Services
{
    /// <summary>
    /// Every salary number in the module comes from here, so the formulas live in
    /// exactly one place:
    ///
    ///   hourly rate     = basic salary / 30 / shift hours
    ///   net hours       = overtime hours − late hours
    ///   overtime amount = net hours × hourly rate
    ///   earned basic    = basic salary / 30 × working days
    ///   total advance   = previous balance + current advance
    ///   remaining       = total advance − advance deducted
    ///   net payable     = earned basic − advance deducted − fixed monthly deduction
    ///                     (− tax / statutory / other when those are entered)
    ///
    /// Overtime amount is computed and shown on every payslip but is NOT added to
    /// net pay by default, matching the agreed formula. Tick "Include overtime
    /// amount in net pay" on the generate / pay-run screen to fold it in.
    /// </summary>
    public interface IPayrollService
    {
        Task<OtherPayrollSettings> GetSettingsAsync();
        Task<MachineIntegrationSetting> GetMachineIntegrationAsync();

        Task<SalaryComputationViewModel> ComputeAsync(int employeeId, int month, int year, bool includeOvertimeInNetPay = false);

        Task<List<SalaryComputationViewModel>> ComputeManyAsync(int? departmentId, int? employeeId, int month, int year, bool includeOvertimeInNetPay = false);

        /// <summary>Progressive slab tax on an annual taxable income, with a per-slab breakdown for the calculator screen.</summary>
        Task<(decimal AnnualTax, List<TaxSlabBreakdownRow> Breakdown)> CalculateAnnualTaxAsync(decimal annualTaxableIncome);

        /// <summary>Outstanding (not yet recovered) advance for an employee, taken from their most recent payslip.</summary>
        Task<decimal> GetOutstandingAdvanceAsync(int employeeId);

        Task<string> GenerateNumberAsync(string prefix, Func<string, Task<bool>> exists);
    }

    public class PayrollService : IPayrollService
    {
        private const decimal StandardMonthDays = 30m;
        private const decimal DefaultShiftHours = 8m;

        private readonly ApplicationDbContext _db;

        public PayrollService(ApplicationDbContext db) => _db = db;

        // ---------------- Singleton settings (self-seeding) ----------------

        public async Task<OtherPayrollSettings> GetSettingsAsync()
        {
            var settings = await _db.OtherPayrollSettings.OrderBy(s => s.Id).FirstOrDefaultAsync();
            if (settings != null) return settings;

            settings = new OtherPayrollSettings();
            _db.OtherPayrollSettings.Add(settings);
            await _db.SaveChangesAsync();
            return settings;
        }

        public async Task<MachineIntegrationSetting> GetMachineIntegrationAsync()
        {
            var settings = await _db.MachineIntegrationSettings.OrderBy(s => s.Id).FirstOrDefaultAsync();
            if (settings != null) return settings;

            settings = new MachineIntegrationSetting();
            _db.MachineIntegrationSettings.Add(settings);
            await _db.SaveChangesAsync();
            return settings;
        }

        // ---------------- Salary computation ----------------

        public async Task<SalaryComputationViewModel> ComputeAsync(int employeeId, int month, int year, bool includeOvertimeInNetPay = false)
        {
            var results = await ComputeManyAsync(null, employeeId, month, year, includeOvertimeInNetPay);
            return results.FirstOrDefault() ?? new SalaryComputationViewModel { EmployeeId = employeeId, Month = month, Year = year };
        }

        public async Task<List<SalaryComputationViewModel>> ComputeManyAsync(int? departmentId, int? employeeId, int month, int year, bool includeOvertimeInNetPay = false)
        {
            month = Math.Clamp(month, 1, 12);
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var daysInMonth = DateTime.DaysInMonth(year, month);

            var settings = await GetSettingsAsync();

            var employeeQuery = _db.Employees
                .Include(e => e.DepartmentRef)
                .Include(e => e.ShiftTiming)
                .Where(e => e.IsActive);

            if (departmentId.HasValue) employeeQuery = employeeQuery.Where(e => e.DepartmentId == departmentId.Value);
            if (employeeId.HasValue) employeeQuery = employeeQuery.Where(e => e.Id == employeeId.Value);

            var employees = await employeeQuery.OrderBy(e => e.Name).ToListAsync();
            if (employees.Count == 0) return new List<SalaryComputationViewModel>();

            var ids = employees.Select(e => e.Id).ToList();

            var attendance = await _db.AttendanceRecords
                .Where(a => ids.Contains(a.EmployeeId) && a.Date >= monthStart && a.Date <= monthEnd)
                .ToListAsync();

            // Advances / mid-salary approved on or before the end of this month.
            var loans = await _db.LoanAdvanceRequests
                .Where(l => ids.Contains(l.EmployeeId) && l.Status == VoucherStatus.Approved && l.ApprovedOn != null && l.ApprovedOn <= monthEnd)
                .ToListAsync();

            var midSalaries = await _db.MidSalaryRequests
                .Where(m => ids.Contains(m.EmployeeId) && m.Status == VoucherStatus.Approved && m.ApprovedOn != null
                            && m.ApprovedOn >= monthStart && m.ApprovedOn <= monthEnd)
                .ToListAsync();

            // Prior payslips carry the advance balance forward.
            var priorPayslips = await _db.Payslips
                .Where(p => ids.Contains(p.EmployeeId) && p.Status != VoucherStatus.Rejected
                            && (p.Year < year || (p.Year == year && p.Month < month)))
                .ToListAsync();

            var existingPayslips = await _db.Payslips
                .Where(p => ids.Contains(p.EmployeeId) && p.Year == year && p.Month == month && p.Status != VoucherStatus.Rejected)
                .ToListAsync();

            var results = new List<SalaryComputationViewModel>();

            foreach (var employee in employees)
            {
                var shiftHours = employee.ShiftTiming?.ShiftHours ?? DefaultShiftHours;
                if (shiftHours <= 0) shiftHours = DefaultShiftHours;

                var vm = new SalaryComputationViewModel
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.Name,
                    DepartmentName = employee.DepartmentRef?.Name ?? employee.Department,
                    Designation = employee.Designation,
                    ShiftName = employee.ShiftTiming?.Name,
                    Month = month,
                    Year = year,
                    BasicSalary = employee.BasicSalary,
                    HouseRentAllowance = employee.HouseRentAllowance,
                    MedicalAllowance = employee.MedicalAllowance,
                    OtherAllowances = employee.OtherAllowances,
                    ShiftHours = shiftHours,
                    FixedMonthlyDeduction = settings.MonthlyFixedDeduction,
                    IncludeOvertimeInNetPay = includeOvertimeInNetPay
                };

                if (employee.ShiftTiming == null)
                    vm.Notes.Add($"No shift assigned — {DefaultShiftHours:0} hours assumed for the overtime rate.");

                if (employee.BasicSalary <= 0)
                    vm.Notes.Add("Basic salary is zero — set it on the employee record.");

                vm.HourlyRate = Math.Round(employee.BasicSalary / StandardMonthDays / shiftHours, 4);

                // ----- Attendance-driven figures -----
                var records = attendance.Where(a => a.EmployeeId == employee.Id).ToList();

                if (records.Count == 0)
                {
                    // No attendance captured at all: treat the month as fully worked rather
                    // than paying zero, and say so on screen.
                    vm.WorkingDays = daysInMonth;
                    vm.PresentDays = daysInMonth;
                    vm.Notes.Add("No attendance recorded for this month — a full month has been assumed.");
                }
                else
                {
                    vm.PresentDays = records.Count(r => r.Status == AttendanceStatus.Present);
                    vm.AbsentDays = records.Count(r => r.Status == AttendanceStatus.Absent);
                    vm.LeaveDays = records.Count(r => r.Status == AttendanceStatus.Leave);
                    var halfDays = records.Count(r => r.Status == AttendanceStatus.HalfDay);
                    var holidayOrOff = records.Count(r => r.Status is AttendanceStatus.Holiday or AttendanceStatus.WeekOff);

                    // Payable days = worked + paid leave + holidays/week-offs, half days at 50%.
                    var payable = vm.PresentDays + vm.LeaveDays + holidayOrOff + (halfDays / 2m);
                    vm.WorkingDays = (int)Math.Round(payable, MidpointRounding.AwayFromZero);

                    var uncaptured = daysInMonth - records.Count;
                    if (uncaptured > 0)
                        vm.Notes.Add($"{uncaptured} day(s) of this month have no attendance entry and are unpaid.");

                    vm.LateHours = Math.Round(records.Sum(r => r.LateByMinutes) / 60m, 2);
                    var breakMinutes = employee.ShiftTiming?.BreakMinutes ?? 0;
                    vm.OvertimeHours = Math.Round(records.Sum(r => AttendanceTimeCalculator.OvertimeHours(r, shiftHours, breakMinutes)), 2);
                }

                // ----- Overtime -----
                vm.NetHours = Math.Round(vm.OvertimeHours - vm.LateHours, 2);
                vm.OvertimeAmount = Math.Round(vm.NetHours * vm.HourlyRate, 2);

                // ----- Earned basic -----
                vm.EarnedBasic = Math.Round(employee.BasicSalary / StandardMonthDays * vm.WorkingDays, 2);
                vm.AbsentDeduction = Math.Round(employee.BasicSalary - vm.EarnedBasic, 2);
                if (vm.AbsentDeduction < 0) vm.AbsentDeduction = 0;

                // ----- Advances -----
                var prior = priorPayslips
                    .Where(p => p.EmployeeId == employee.Id)
                    .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                    .FirstOrDefault();

                vm.AdvancePreviousBalance = prior?.AdvanceRemainingBalance ?? 0m;

                var employeeLoans = loans.Where(l => l.EmployeeId == employee.Id).ToList();
                var newLoansThisMonth = employeeLoans
                    .Where(l => l.ApprovedOn >= monthStart && l.ApprovedOn <= monthEnd)
                    .Sum(l => l.Amount);
                var midSalaryThisMonth = midSalaries.Where(m => m.EmployeeId == employee.Id).Sum(m => m.Amount);

                vm.CurrentAdvanceGiven = newLoansThisMonth + midSalaryThisMonth;
                vm.TotalAdvance = vm.AdvancePreviousBalance + vm.CurrentAdvanceGiven;

                // Suggested recovery: each loan contributes Amount / InstallmentMonths for as
                // many months as its recovery window covers; mid-salary is recovered in full.
                var installment = 0m;
                foreach (var loan in employeeLoans)
                {
                    var start = loan.ApprovedOn!.Value;
                    var monthsElapsed = ((year - start.Year) * 12) + (month - start.Month);
                    if (monthsElapsed >= 0 && monthsElapsed < Math.Max(1, loan.InstallmentMonths))
                        installment += loan.Amount / Math.Max(1, loan.InstallmentMonths);
                }
                installment += midSalaryThisMonth;

                vm.AdvanceDeducted = Math.Round(Math.Min(vm.TotalAdvance, Math.Max(0, installment)), 2);
                vm.AdvanceRemainingBalance = Math.Round(vm.TotalAdvance - vm.AdvanceDeducted, 2);

                // ----- Deductions and net pay -----
                // Income tax / statutory / other default to zero so net pay matches the
                // agreed formula; they can be entered per payslip when required.
                vm.TotalDeductions = Math.Round(
                    vm.AdvanceDeducted + vm.FixedMonthlyDeduction + vm.IncomeTaxDeduction +
                    vm.StatutoryDeductions + vm.OtherDeductions, 2);

                var gross = vm.EarnedBasic + (includeOvertimeInNetPay ? vm.OvertimeAmount : 0m);
                var net = gross - vm.TotalDeductions;
                vm.NetPay = settings.RoundOffNetPay ? Math.Round(net, 0, MidpointRounding.AwayFromZero) : Math.Round(net, 2);

                if (vm.NetPay < 0)
                    vm.Notes.Add("Net pay is negative — the advance recovery exceeds this month's earnings.");

                var existing = existingPayslips.FirstOrDefault(p => p.EmployeeId == employee.Id);
                vm.AlreadyGenerated = existing != null;
                vm.ExistingPayslipId = existing?.Id;

                results.Add(vm);
            }

            return results;
        }

        // ---------------- Advances ----------------

        public async Task<decimal> GetOutstandingAdvanceAsync(int employeeId)
        {
            var latest = await _db.Payslips
                .Where(p => p.EmployeeId == employeeId && p.Status != VoucherStatus.Rejected)
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .FirstOrDefaultAsync();

            if (latest != null) return latest.AdvanceRemainingBalance;

            // Nothing has been paid yet — everything approved is still outstanding.
            var loans = await _db.LoanAdvanceRequests
                .Where(l => l.EmployeeId == employeeId && l.Status == VoucherStatus.Approved)
                .SumAsync(l => (decimal?)l.Amount) ?? 0m;

            var mid = await _db.MidSalaryRequests
                .Where(m => m.EmployeeId == employeeId && m.Status == VoucherStatus.Approved)
                .SumAsync(m => (decimal?)m.Amount) ?? 0m;

            return loans + mid;
        }

        // ---------------- Tax ----------------

        public async Task<(decimal AnnualTax, List<TaxSlabBreakdownRow> Breakdown)> CalculateAnnualTaxAsync(decimal annualTaxableIncome)
        {
            if (annualTaxableIncome < 0) annualTaxableIncome = 0;

            var slabs = await _db.TaxSlabs
                .Where(s => s.IsActive)
                .OrderBy(s => s.MinIncome)
                .ToListAsync();

            var breakdown = new List<TaxSlabBreakdownRow>();
            var applicable = slabs
                .LastOrDefault(s => annualTaxableIncome > s.MinIncome && (s.MaxIncome == null || annualTaxableIncome <= s.MaxIncome));

            // Income that falls in the very first slab (typically the zero-rated band).
            if (applicable == null && slabs.Count > 0)
                applicable = slabs.First();

            foreach (var slab in slabs)
            {
                var isApplicable = applicable != null && slab.Id == applicable.Id;
                var taxableHere = isApplicable ? Math.Max(0, annualTaxableIncome - slab.MinIncome) : 0m;
                var taxHere = isApplicable ? slab.FixedAmount + (taxableHere * slab.RatePercent / 100m) : 0m;

                breakdown.Add(new TaxSlabBreakdownRow
                {
                    Range = slab.MaxIncome == null
                        ? $"Above {slab.MinIncome:N0}"
                        : $"{slab.MinIncome:N0} – {slab.MaxIncome.Value:N0}",
                    RatePercent = slab.RatePercent,
                    TaxableInThisSlab = Math.Round(taxableHere, 2),
                    TaxOnThisSlab = Math.Round(taxHere, 2),
                    IsApplicableSlab = isApplicable
                });
            }

            var annualTax = Math.Round(breakdown.Sum(b => b.TaxOnThisSlab), 2);
            return (annualTax, breakdown);
        }

        // ---------------- Numbering ----------------

        public async Task<string> GenerateNumberAsync(string prefix, Func<string, Task<bool>> exists)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMM");
            var counter = 1;
            string candidate;

            do
            {
                candidate = $"{prefix}-{stamp}-{counter:D4}";
                counter++;
            }
            while (await exists(candidate) && counter < 100000);

            return candidate;
        }
    }
}
