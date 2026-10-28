using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Models.ViewModels.Accounts;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>
    /// A pay run is one batch of payslips for a period, approved together. Approving
    /// the run approves every payslip inside it, which is what posts salaries, advance
    /// recoveries and the fixed monthly deduction to employee ledgers.
    /// </summary>
    [Authorize]
    [Route("Payroll/PayRuns")]
    public class PayRunsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountingService _accountingService;

        public PayRunsController(ApplicationDbContext db, IPayrollService payrollService, UserManager<ApplicationUser> userManager, IAccountingService accountingService)
        {
            _db = db;
            _payrollService = payrollService;
            _userManager = userManager;
            _accountingService = accountingService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View();
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var runs = await _db.PayRuns.OrderByDescending(r => r.Id).ToListAsync();

            var list = runs.Select(r => new PayRunListItemViewModel
            {
                Id = r.Id,
                RunNumber = r.RunNumber,
                Period = new DateTime(r.Year, r.Month, 1).ToString("MMM yyyy"),
                TotalEmployees = r.TotalEmployees,
                TotalGrossAmount = r.TotalGrossAmount,
                TotalDeductions = r.TotalDeductions,
                TotalNetAmount = r.TotalNetAmount,
                Status = r.Status.ToString(),
                CreatedOn = r.CreatedOn
            }).ToList();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Create()
        {
            var lastMonth = DateTime.Today.AddMonths(-1);
            var vm = new PayRunCreateViewModel { Month = lastMonth.Month, Year = lastMonth.Year };

            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        [HttpPost("Preview")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Preview(PayRunCreateViewModel model)
        {
            if (model.PayImmediately && !await IsPaymentAccountAsync(model.PaymentAccountId))
                ModelState.AddModelError(nameof(model.PaymentAccountId), "Choose an active cash or bank account for immediate payment.");

            if (!ModelState.IsValid)
            {
                model.HasPreview = false;
                await PopulateDropdownsAsync(model);
                return View("Create", model);
            }

            model.Preview = await _payrollService.ComputeManyAsync(
                model.DepartmentId, null, model.Month, model.Year, model.IncludeOvertimeInNetPay);

            model.HasPreview = true;
            await PopulateDropdownsAsync(model);

            if (model.Preview.Count == 0)
                TempData["ErrorMessage"] = "No active employees matched that filter.";

            return View("Create", model);
        }

        [HttpPost("Confirm")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Confirm(int month, int year, int? departmentId, bool includeOvertimeInNetPay, bool payImmediately, int? paymentAccountId)
        {
            if (payImmediately && !await IsPaymentAccountAsync(paymentAccountId))
            {
                TempData["ErrorMessage"] = "Choose an active cash or bank account for immediate payment.";
                return RedirectToAction(nameof(Create));
            }

            if (await _db.PayRuns.AnyAsync(r => r.Month == month && r.Year == year && r.Status != VoucherStatus.Rejected))
            {
                TempData["ErrorMessage"] = $"A pay run already exists for {new DateTime(year, month, 1):MMM yyyy}.";
                return RedirectToAction(nameof(Index));
            }

            var computations = await _payrollService.ComputeManyAsync(departmentId, null, month, year, includeOvertimeInNetPay);
            var eligible = computations.Where(c => !c.AlreadyGenerated).ToList();

            if (eligible.Count == 0)
            {
                TempData["ErrorMessage"] = "Every matching employee already has a payslip for this period.";
                return RedirectToAction(nameof(Index));
            }

            var run = new PayRun
            {
                RunNumber = await _payrollService.GenerateNumberAsync("RUN",
                    n => _db.PayRuns.AnyAsync(r => r.RunNumber == n)),
                Month = month,
                Year = year,
                PayImmediately = payImmediately,
                PaymentAccountId = payImmediately ? paymentAccountId : null,
                Status = VoucherStatus.Pending,
                CreatedOn = DateTime.UtcNow
            };

            _db.PayRuns.Add(run);
            await _db.SaveChangesAsync();

            foreach (var c in eligible)
            {
                var payslip = new Payslip
                {
                    PayslipNumber = await _payrollService.GenerateNumberAsync("PS",
                        n => _db.Payslips.AnyAsync(p => p.PayslipNumber == n)),
                    PayRunId = run.Id,
                    EmployeeId = c.EmployeeId,
                    Month = month,
                    Year = year,
                    Status = VoucherStatus.Pending,
                    CreatedOn = DateTime.UtcNow,

                    BasicSalary = c.BasicSalary,
                    HouseRentAllowance = c.HouseRentAllowance,
                    MedicalAllowance = c.MedicalAllowance,
                    OtherAllowances = c.OtherAllowances,
                    GrossSalary = c.EarnedBasic + c.HouseRentAllowance + c.MedicalAllowance + c.OtherAllowances,

                    WorkingDays = c.WorkingDays,
                    PresentDays = c.PresentDays,
                    AbsentDays = c.AbsentDays,
                    LeaveDays = c.LeaveDays,

                    OvertimeHours = c.OvertimeHours,
                    LateHours = c.LateHours,
                    NetHours = c.NetHours,
                    OvertimeAmount = c.OvertimeAmount,

                    AdvancePreviousBalance = c.AdvancePreviousBalance,
                    CurrentAdvanceGiven = c.CurrentAdvanceGiven,
                    TotalAdvance = c.TotalAdvance,
                    AdvanceDeducted = c.AdvanceDeducted,
                    AdvanceRemainingBalance = c.AdvanceRemainingBalance,

                    FixedMonthlyDeduction = c.FixedMonthlyDeduction,
                    IncomeTaxDeduction = c.IncomeTaxDeduction,
                    StatutoryDeductions = c.StatutoryDeductions,
                    LoanDeduction = c.AdvanceDeducted,
                    AbsentDeduction = c.AbsentDeduction,
                    OtherDeductions = c.OtherDeductions,
                    TotalDeductions = c.TotalDeductions,
                    NetPay = c.NetPay
                };

                _db.Payslips.Add(payslip);
                await _db.SaveChangesAsync();
            }

            run.TotalEmployees = eligible.Count;
            run.TotalGrossAmount = eligible.Sum(c => c.EarnedBasic + (includeOvertimeInNetPay ? c.OvertimeAmount : 0m));
            run.TotalDeductions = eligible.Sum(c => c.TotalDeductions);
            run.TotalNetAmount = eligible.Sum(c => c.NetPay);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Pay run {run.RunNumber} created with {eligible.Count} payslip(s), net {run.TotalNetAmount:N2}.";
            return RedirectToAction(nameof(View), new { id = run.Id });
        }

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var run = await _db.PayRuns
                .Include(r => r.Payslips).ThenInclude(p => p.Employee).ThenInclude(e => e!.DepartmentRef)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (run == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View(run);
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var run = await _db.PayRuns.Include(r => r.Payslips).FirstOrDefaultAsync(r => r.Id == id);
            if (run == null) return Json(new { success = false, message = "Pay run not found." });
            if (run.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending pay runs can be approved." });

            if (await _db.JournalEntries.AnyAsync(entry => entry.SourceType == "PayRun" && entry.SourceId == run.Id))
                return Json(new { success = false, message = "A payroll journal already exists for this run." });

            var salaryExpense = await _db.Accounts.FirstOrDefaultAsync(account => account.IsActive && account.AccountType == AccountType.Expense && account.AccountName == "Salaries & Wages");
            var payrollPayable = await _db.Accounts.FirstOrDefaultAsync(account => account.IsActive && account.AccountType == AccountType.Liability && account.AccountName == "Payroll Payable");
            var deductionsPayable = await _db.Accounts.FirstOrDefaultAsync(account => account.IsActive && account.AccountType == AccountType.Liability && account.AccountName == "Payroll Deductions Payable");
            var paymentAccount = run.PayImmediately && run.PaymentAccountId.HasValue
                ? await _db.Accounts.FirstOrDefaultAsync(account => account.Id == run.PaymentAccountId.Value && account.IsActive && account.AccountType == AccountType.Asset)
                : null;

            if (salaryExpense == null || payrollPayable == null || deductionsPayable == null || (run.PayImmediately && paymentAccount == null))
                return Json(new { success = false, message = "Payroll expense, payable, deduction, and selected payment accounts must be active in the chart of accounts." });

            var lines = new List<JournalLineInput>();
            foreach (var payslip in run.Payslips.Where(payslip => payslip.Status == VoucherStatus.Pending))
            {
                if (payslip.NetPay < 0)
                    return Json(new { success = false, message = $"Payslip {payslip.PayslipNumber} has negative net pay. Correct it before approving this run." });

                var netPay = Math.Round(payslip.NetPay, 2);
                var grossPay = Math.Round(netPay + payslip.TotalDeductions, 2);
                var withheld = Math.Max(0m, grossPay - netPay);
                if (grossPay <= 0) continue;

                lines.Add(new JournalLineInput(salaryExpense.Id, grossPay, 0m, Narration: $"Payroll expense — {payslip.PayslipNumber}"));
                if (netPay > 0)
                {
                    lines.Add(new JournalLineInput(payrollPayable.Id, 0m, netPay, PartyType.Employee, payslip.EmployeeId,
                        $"Net salary payable — {payslip.PayslipNumber}"));
                    if (run.PayImmediately)
                    {
                        lines.Add(new JournalLineInput(payrollPayable.Id, netPay, 0m, PartyType.Employee, payslip.EmployeeId,
                            $"Salary paid — {payslip.PayslipNumber}"));
                        lines.Add(new JournalLineInput(paymentAccount!.Id, 0m, netPay, Narration: $"Salary payment — {payslip.PayslipNumber}"));
                    }
                }
                if (withheld > 0)
                    lines.Add(new JournalLineInput(deductionsPayable.Id, 0m, withheld, Narration: $"Payroll deductions — {payslip.PayslipNumber}"));
            }

            if (lines.Count == 0)
                return Json(new { success = false, message = "This run has no positive payroll amounts to post." });

            var user = await _userManager.GetUserAsync(User);
            var stamp = DateTime.UtcNow;

            await using var transaction = await _db.Database.BeginTransactionAsync();
            await _accountingService.PostJournalEntryAsync("PayRun", run.Id,
                new DateTime(run.Year, run.Month, DateTime.DaysInMonth(run.Year, run.Month)),
                $"Payroll {new DateTime(run.Year, run.Month, 1):MMMM yyyy} — {run.RunNumber}", lines);

            run.Status = VoucherStatus.Approved;
            run.ApprovedById = user?.Id;
            run.ApprovedByName = user?.FullName;
            run.ApprovedOn = stamp;

            foreach (var payslip in run.Payslips.Where(p => p.Status == VoucherStatus.Pending))
            {
                payslip.Status = VoucherStatus.Approved;
                payslip.ApprovedById = user?.Id;
                payslip.ApprovedByName = user?.FullName;
                payslip.ApprovedOn = stamp;
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new
            {
                success = true,
                message = run.PayImmediately
                    ? $"Pay run {run.RunNumber} approved, paid from {paymentAccount!.AccountName}, and posted to the general and employee ledgers."
                    : $"Pay run {run.RunNumber} approved and posted to the general and employee ledgers as payable."
            });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest payload)
        {
            var run = await _db.PayRuns.Include(r => r.Payslips).FirstOrDefaultAsync(r => r.Id == payload.Id);
            if (run == null) return Json(new { success = false, message = "Pay run not found." });
            if (run.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending pay runs can be rejected." });

            var user = await _userManager.GetUserAsync(User);
            var stamp = DateTime.UtcNow;

            run.Status = VoucherStatus.Rejected;
            run.ApprovedById = user?.Id;
            run.ApprovedByName = user?.FullName;
            run.ApprovedOn = stamp;
            run.RejectionReason = payload.Reason;

            foreach (var payslip in run.Payslips.Where(p => p.Status == VoucherStatus.Pending))
            {
                payslip.Status = VoucherStatus.Rejected;
                payslip.ApprovedById = user?.Id;
                payslip.ApprovedByName = user?.FullName;
                payslip.ApprovedOn = stamp;
                payslip.RejectionReason = payload.Reason;
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Pay run {run.RunNumber} and its payslips were rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var run = await _db.PayRuns.Include(r => r.Payslips).FirstOrDefaultAsync(r => r.Id == id);
            if (run == null) return Json(new { success = false, message = "Pay run not found." });
            if (run.Status == VoucherStatus.Approved)
                return Json(new { success = false, message = "Approved pay runs cannot be deleted — their payslips carry advance balances forward." });

            // Payslips cascade with the run (configured in ApplicationDbContext).
            _db.PayRuns.Remove(run);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Pay run and its payslips deleted." });
        }

        private async Task PopulateDropdownsAsync(PayRunCreateViewModel vm)
        {
            vm.Departments = await _db.Departments.Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentOption(d.Id, d.Name, d.Code))
                .ToListAsync();

            vm.PaymentAccounts = await _db.Accounts
                .Where(account => account.IsActive && account.AccountType == AccountType.Asset &&
                    (account.AccountName.Contains("Cash") || account.AccountName.Contains("Bank")))
                .OrderBy(account => account.AccountCode)
                .Select(account => new AccountOption(account.Id, account.AccountCode, account.AccountName, account.AccountType))
                .ToListAsync();
        }

        private async Task<bool> IsPaymentAccountAsync(int? accountId) => accountId.HasValue &&
            await _db.Accounts.AnyAsync(account => account.Id == accountId.Value && account.IsActive &&
                account.AccountType == AccountType.Asset && (account.AccountName.Contains("Cash") || account.AccountName.Contains("Bank")));
    }
}
