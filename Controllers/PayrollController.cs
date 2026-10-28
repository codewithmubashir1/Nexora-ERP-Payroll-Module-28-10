using ERP.Identity.Data;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Payroll landing page: headline counts, pending approvals and quick actions.</summary>
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;

        public PayrollController(ApplicationDbContext db, IPayrollService payrollService)
        {
            _db = db;
            _payrollService = payrollService;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var lastMonth = today.AddMonths(-1);
            var settings = await _payrollService.GetSettingsAsync();

            ViewBag.EmployeeCount = await _db.Employees.CountAsync(e => e.IsActive);
            ViewBag.DepartmentCount = await _db.Departments.CountAsync(d => d.IsActive);

            ViewBag.PendingRequests =
                await _db.LeaveRequests.CountAsync(r => r.Status == VoucherStatus.Pending) +
                await _db.LoanAdvanceRequests.CountAsync(r => r.Status == VoucherStatus.Pending) +
                await _db.MidSalaryRequests.CountAsync(r => r.Status == VoucherStatus.Pending) +
                await _db.LeaveEncashmentRequests.CountAsync(r => r.Status == VoucherStatus.Pending) +
                await _db.ReimbursementRequests.CountAsync(r => r.Status == VoucherStatus.Pending);

            ViewBag.PendingPayslips = await _db.Payslips.CountAsync(p => p.Status == VoucherStatus.Pending);

            ViewBag.LastMonthName = lastMonth.ToString("MMM yyyy");
            ViewBag.LastMonthNetPayroll = await _db.Payslips
                .Where(p => p.Month == lastMonth.Month && p.Year == lastMonth.Year && p.Status != VoucherStatus.Rejected)
                .SumAsync(p => (decimal?)p.NetPay) ?? 0m;

            // Each employee's outstanding advance is whatever their most recent payslip
            // carried forward, so the balances are summed in memory after loading the
            // minimal projection rather than in a grouped SQL query.
            var balances = await _db.Payslips
                .Where(p => p.Status != VoucherStatus.Rejected)
                .Select(p => new { p.EmployeeId, p.Year, p.Month, p.AdvanceRemainingBalance })
                .ToListAsync();

            ViewBag.OutstandingAdvances = balances
                .GroupBy(p => p.EmployeeId)
                .Select(g => g.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).First().AdvanceRemainingBalance)
                .Sum();

            ViewBag.TodayPresent = await _db.AttendanceRecords
                .CountAsync(a => a.Date == today && a.Status == AttendanceStatus.Present);

            ViewBag.TodayMarked = await _db.AttendanceRecords.CountAsync(a => a.Date == today);

            ViewBag.MonthlyFixedDeduction = settings.MonthlyFixedDeduction;

            ViewBag.UpcomingHolidays = await _db.Holidays
                .Where(h => h.Date >= today)
                .OrderBy(h => h.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
