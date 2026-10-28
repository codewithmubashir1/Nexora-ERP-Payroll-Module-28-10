using System.Text;
using ERP.Identity.Data;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>
    /// The month's payroll register — one row per payslip with every component of the
    /// salary formula laid out side by side, plus a bank-transfer style CSV export.
    /// </summary>
    [Authorize]
    [Route("Payroll/SalarySheet")]
    public class SalarySheetController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SalarySheetController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public async Task<IActionResult> Index(int? month, int? year, int? departmentId)
        {
            var vm = await BuildAsync(month, year, departmentId);
            return View(vm);
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export(int? month, int? year, int? departmentId)
        {
            var vm = await BuildAsync(month, year, departmentId);

            var sb = new StringBuilder();
            sb.AppendLine("Payslip #,Employee Code,Employee,Department,Designation,Bank,Account Number," +
                          "Basic Salary,Working Days,Attendance Hours,Earned Basic,Overtime Hours,Late Hours,Net Hours,Overtime Amount," +
                          "Advance Previous Balance,Current Advance,Total Advance,Advance Deducted,Advance Remaining," +
                          "Fixed Deduction,Total Deductions,Net Payable,Status");

            foreach (var r in vm.Rows)
            {
                sb.Append(Escape(r.PayslipNumber)).Append(',')
                  .Append(Escape(r.EmployeeCode)).Append(',')
                  .Append(Escape(r.EmployeeName)).Append(',')
                  .Append(Escape(r.DepartmentName)).Append(',')
                  .Append(Escape(r.Designation)).Append(',')
                  .Append(Escape(r.BankName)).Append(',')
                  .Append(Escape(r.BankAccountNumber)).Append(',')
                  .Append(r.BasicSalary.ToString("0.00")).Append(',')
                  .Append(r.WorkingDays).Append(',')
                  .Append(r.AttendanceHours.ToString("0.00")).Append(',')
                  .Append(r.EarnedBasic.ToString("0.00")).Append(',')
                  .Append(r.OvertimeHours.ToString("0.00")).Append(',')
                  .Append(r.LateHours.ToString("0.00")).Append(',')
                  .Append(r.NetHours.ToString("0.00")).Append(',')
                  .Append(r.OvertimeAmount.ToString("0.00")).Append(',')
                  .Append(r.AdvancePreviousBalance.ToString("0.00")).Append(',')
                  .Append(r.CurrentAdvanceGiven.ToString("0.00")).Append(',')
                  .Append(r.TotalAdvance.ToString("0.00")).Append(',')
                  .Append(r.AdvanceDeducted.ToString("0.00")).Append(',')
                  .Append(r.AdvanceRemainingBalance.ToString("0.00")).Append(',')
                  .Append(r.FixedMonthlyDeduction.ToString("0.00")).Append(',')
                  .Append(r.TotalDeductions.ToString("0.00")).Append(',')
                  .Append(r.NetPay.ToString("0.00")).Append(',')
                  .Append(r.Status)
                  .AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"salary_sheet_{vm.Year}_{vm.Month:D2}.csv");
        }

        private async Task<SalarySheetViewModel> BuildAsync(int? month, int? year, int? departmentId)
        {
            var vm = new SalarySheetViewModel
            {
                Month = Math.Clamp(month ?? DateTime.Today.AddMonths(-1).Month, 1, 12),
                Year = year ?? DateTime.Today.AddMonths(-1).Year,
                DepartmentId = departmentId
            };

            vm.Departments = await _db.Departments.Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentOption(d.Id, d.Name, d.Code))
                .ToListAsync();

            var query = _db.Payslips
                .Include(p => p.Employee).ThenInclude(e => e!.DepartmentRef)
                .Include(p => p.Employee).ThenInclude(e => e!.ShiftTiming)
                .Where(p => p.Month == vm.Month && p.Year == vm.Year && p.Status != VoucherStatus.Rejected);

            if (departmentId.HasValue)
                query = query.Where(p => p.Employee!.DepartmentId == departmentId.Value);

            var payslips = await query.OrderBy(p => p.Employee!.Name).ToListAsync();
            var employeeIds = payslips.Select(p => p.EmployeeId).Distinct().ToList();
            var attendance = await _db.AttendanceRecords
                .Where(a => employeeIds.Contains(a.EmployeeId) && a.Date >= new DateTime(vm.Year, vm.Month, 1) && a.Date < new DateTime(vm.Year, vm.Month, 1).AddMonths(1))
                .ToListAsync();
            var breakMinutesByEmployee = payslips
                .GroupBy(p => p.EmployeeId)
                .ToDictionary(g => g.Key, g => g.First().Employee?.ShiftTiming?.BreakMinutes ?? 0);
            var attendanceHoursByEmployee = attendance
                .GroupBy(a => a.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Sum(a => AttendanceTimeCalculator.WorkedHours(a, breakMinutesByEmployee.GetValueOrDefault(a.EmployeeId))));

            vm.Rows = payslips.Select(p => new SalarySheetRowViewModel
            {
                PayslipId = p.Id,
                PayslipNumber = p.PayslipNumber,
                EmployeeCode = p.Employee?.EmployeeCode ?? "—",
                EmployeeName = p.Employee?.Name ?? "—",
                DepartmentName = p.Employee?.DepartmentRef?.Name ?? p.Employee?.Department,
                Designation = p.Employee?.Designation,
                BankName = p.Employee?.BankName,
                BankAccountNumber = p.Employee?.BankAccountNumber,
                BasicSalary = p.BasicSalary,
                WorkingDays = p.WorkingDays,
                AttendanceHours = attendanceHoursByEmployee.GetValueOrDefault(p.EmployeeId),
                EarnedBasic = Math.Round(p.BasicSalary / 30m * p.WorkingDays, 2),
                OvertimeHours = p.OvertimeHours,
                LateHours = p.LateHours,
                NetHours = p.NetHours,
                OvertimeAmount = p.OvertimeAmount,
                AdvancePreviousBalance = p.AdvancePreviousBalance,
                CurrentAdvanceGiven = p.CurrentAdvanceGiven,
                TotalAdvance = p.TotalAdvance,
                AdvanceDeducted = p.AdvanceDeducted,
                AdvanceRemainingBalance = p.AdvanceRemainingBalance,
                FixedMonthlyDeduction = p.FixedMonthlyDeduction,
                TotalDeductions = p.TotalDeductions,
                NetPay = p.NetPay,
                Status = p.Status.ToString()
            }).ToList();

            return vm;
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        }
    }
}
