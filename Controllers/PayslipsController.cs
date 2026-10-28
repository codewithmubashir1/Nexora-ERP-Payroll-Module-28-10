using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ERP.Identity.Controllers
{
    /// <summary>
    /// Payslip generation. The numbers are never typed in: IPayrollService computes
    /// them from attendance, shift length and approved advances, and this controller
    /// only persists the result and moves it through approval.
    /// </summary>
    [Authorize]
    [Route("Payroll/Payslips")]
    public class PayslipsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;
        private readonly ICompanyInfoService _companyInfoService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PayslipsController(
            ApplicationDbContext db,
            IPayrollService payrollService,
            ICompanyInfoService companyInfoService,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _payrollService = payrollService;
            _companyInfoService = companyInfoService;
            _userManager = userManager;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View();
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(int? month, int? year)
        {
            var query = _db.Payslips
                .Include(p => p.Employee).ThenInclude(e => e!.DepartmentRef)
                .Include(p => p.PayRun)
                .AsQueryable();

            if (month.HasValue) query = query.Where(p => p.Month == month.Value);
            if (year.HasValue) query = query.Where(p => p.Year == year.Value);

            var payslips = await query
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ThenBy(p => p.Employee!.Name)
                .ToListAsync();

            var list = payslips.Select(p => new PayslipListItemViewModel
            {
                Id = p.Id,
                PayslipNumber = p.PayslipNumber,
                EmployeeName = p.Employee?.Name ?? "—",
                EmployeeCode = p.Employee?.EmployeeCode ?? "—",
                DepartmentName = p.Employee?.DepartmentRef?.Name ?? p.Employee?.Department,
                Period = new DateTime(p.Year, p.Month, 1).ToString("MMM yyyy"),
                PayRunNumber = p.PayRun?.RunNumber,
                BasicSalary = p.BasicSalary,
                OvertimeAmount = p.OvertimeAmount,
                TotalDeductions = p.TotalDeductions,
                NetPay = p.NetPay,
                Status = p.Status.ToString()
            }).ToList();

            return Json(new { data = list });
        }

        [HttpGet("ImportTemplate")]
        [Authorize(Roles = ApproverRoles)]
        public IActionResult ImportTemplate()
        {
            var headers = new[] { "EmployeeCode", "Month", "Year", "IncomeTaxDeduction", "StatutoryDeductions", "OtherDeductions" };
            var example = new[] { "EMP-000001", "9", "2026", "0", "0", "0" };
            return File(CsvUtility.WriteWithHeader(headers, new[] { example }), "text/csv", "payroll_adjustments_template.csv");
        }

        [HttpPost("ImportAdjustments")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> ImportAdjustments(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Choose a non-empty CSV file." });

            var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
            var employees = await _db.Employees.ToListAsync();
            var pendingPayslips = await _db.Payslips.Where(p => p.Status == VoucherStatus.Pending).ToListAsync();
            var errors = new List<string>();
            var updated = 0;

            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                var rowNumber = index + 2;
                var code = row.Get("EmployeeCode");
                var monthText = row.Get("Month");
                var yearText = row.Get("Year");
                if (string.IsNullOrWhiteSpace(code) || !int.TryParse(monthText, out var month) || month is < 1 or > 12 ||
                    !int.TryParse(yearText, out var year) || year is < 2000 or > 2200)
                {
                    errors.Add($"Row {rowNumber}: EmployeeCode, Month (1–12), and Year are required.");
                    continue;
                }

                var employee = employees.FirstOrDefault(item => item.EmployeeCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (employee == null)
                {
                    errors.Add($"Row {rowNumber}: Employee code '{code}' was not found.");
                    continue;
                }
                var payslip = pendingPayslips.FirstOrDefault(item => item.EmployeeId == employee.Id && item.Month == month && item.Year == year);
                if (payslip == null)
                {
                    errors.Add($"Row {rowNumber}: No pending payslip exists for {code} in {month:D2}/{year}.");
                    continue;
                }

                if (!TryImportAmount(row.Get("IncomeTaxDeduction"), payslip.IncomeTaxDeduction, out var tax) ||
                    !TryImportAmount(row.Get("StatutoryDeductions"), payslip.StatutoryDeductions, out var statutory) ||
                    !TryImportAmount(row.Get("OtherDeductions"), payslip.OtherDeductions, out var other))
                {
                    errors.Add($"Row {rowNumber}: deduction values must be non-negative numbers.");
                    continue;
                }

                var grossBasis = payslip.NetPay + payslip.TotalDeductions;
                payslip.IncomeTaxDeduction = tax;
                payslip.StatutoryDeductions = statutory;
                payslip.OtherDeductions = other;
                payslip.TotalDeductions = Math.Round(payslip.AdvanceDeducted + payslip.FixedMonthlyDeduction + tax + statutory + other, 2);
                payslip.NetPay = Math.Round(grossBasis - payslip.TotalDeductions, 2);
                updated++;
            }

            await _db.SaveChangesAsync();
            var summary = $"Updated deductions on {updated} pending payslip(s); skipped {errors.Count}.";
            return Json(new { success = errors.Count == 0, errorCount = errors.Count, summary, errors });
        }

        private static bool TryImportAmount(string? value, decimal currentValue, out decimal amount)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                amount = currentValue;
                return true;
            }
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount) && amount >= 0;
        }

        // ---------------- Generation ----------------

        [HttpGet("Generate")]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Generate()
        {
            var vm = new PayslipGenerateViewModel();

            // Default to the month just ended — that's the one being paid.
            var lastMonth = DateTime.Today.AddMonths(-1);
            vm.Month = lastMonth.Month;
            vm.Year = lastMonth.Year;

            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        [HttpPost("Preview")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Preview(PayslipGenerateViewModel model)
        {
            model.Preview = await _payrollService.ComputeManyAsync(
                model.DepartmentId, model.EmployeeId, model.Month, model.Year, model.IncludeOvertimeInNetPay);

            model.HasPreview = true;
            await PopulateDropdownsAsync(model);

            if (model.Preview.Count == 0)
                TempData["ErrorMessage"] = "No active employees matched that filter.";

            return View("Generate", model);
        }

        [HttpPost("Confirm")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Confirm(int month, int year, int? departmentId, int? employeeId, bool includeOvertimeInNetPay, int[]? selectedEmployeeIds)
        {
            var computations = await _payrollService.ComputeManyAsync(departmentId, employeeId, month, year, includeOvertimeInNetPay);

            if (selectedEmployeeIds is { Length: > 0 })
                computations = computations.Where(c => selectedEmployeeIds.Contains(c.EmployeeId)).ToList();

            var created = 0;
            var skipped = 0;

            foreach (var c in computations)
            {
                if (c.AlreadyGenerated) { skipped++; continue; }

                var payslip = new Payslip
                {
                    PayslipNumber = await _payrollService.GenerateNumberAsync("PS",
                        n => _db.Payslips.AnyAsync(p => p.PayslipNumber == n)),
                    EmployeeId = c.EmployeeId,
                    Month = month,
                    Year = year,
                    Status = VoucherStatus.Pending,
                    CreatedOn = DateTime.UtcNow
                };

                ApplyComputation(payslip, c);
                _db.Payslips.Add(payslip);
                created++;

                // Saved per row so each payslip number is reserved as it is issued.
                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = skipped > 0
                ? $"{created} payslip(s) generated. {skipped} were skipped — they already exist for this period."
                : $"{created} payslip(s) generated and are awaiting approval.";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>Recalculates an existing pending payslip against current attendance and advances.</summary>
        [HttpPost("Recalculate")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Recalculate(int id)
        {
            var payslip = await _db.Payslips.FindAsync(id);
            if (payslip == null) return Json(new { success = false, message = "Payslip not found." });
            if (payslip.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending payslips can be recalculated." });

            var computation = await _payrollService.ComputeAsync(payslip.EmployeeId, payslip.Month, payslip.Year);
            ApplyComputation(payslip, computation);

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Payslip {payslip.PayslipNumber} recalculated. Net pay is now {payslip.NetPay:N2}." });
        }

        private static void ApplyComputation(Payslip payslip, SalaryComputationViewModel c)
        {
            payslip.BasicSalary = c.BasicSalary;
            payslip.HouseRentAllowance = c.HouseRentAllowance;
            payslip.MedicalAllowance = c.MedicalAllowance;
            payslip.OtherAllowances = c.OtherAllowances;
            payslip.GrossSalary = c.EarnedBasic + c.HouseRentAllowance + c.MedicalAllowance + c.OtherAllowances;

            payslip.WorkingDays = c.WorkingDays;
            payslip.PresentDays = c.PresentDays;
            payslip.AbsentDays = c.AbsentDays;
            payslip.LeaveDays = c.LeaveDays;

            payslip.OvertimeHours = c.OvertimeHours;
            payslip.LateHours = c.LateHours;
            payslip.NetHours = c.NetHours;
            payslip.OvertimeAmount = c.OvertimeAmount;

            payslip.AdvancePreviousBalance = c.AdvancePreviousBalance;
            payslip.CurrentAdvanceGiven = c.CurrentAdvanceGiven;
            payslip.TotalAdvance = c.TotalAdvance;
            payslip.AdvanceDeducted = c.AdvanceDeducted;
            payslip.AdvanceRemainingBalance = c.AdvanceRemainingBalance;

            payslip.FixedMonthlyDeduction = c.FixedMonthlyDeduction;
            payslip.IncomeTaxDeduction = c.IncomeTaxDeduction;
            payslip.StatutoryDeductions = c.StatutoryDeductions;
            payslip.LoanDeduction = c.AdvanceDeducted;
            payslip.AbsentDeduction = c.AbsentDeduction;
            payslip.OtherDeductions = c.OtherDeductions;
            payslip.TotalDeductions = c.TotalDeductions;
            payslip.NetPay = c.NetPay;
        }

        // ---------------- View / print ----------------

        [HttpGet("View/{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var payslip = await LoadFullAsync(id);
            if (payslip == null) return NotFound();

            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View(payslip);
        }

        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var payslip = await LoadFullAsync(id);
            if (payslip == null) return NotFound();

            ViewBag.Company = await _companyInfoService.GetAsync();
            return View(payslip);
        }

        // ---------------- Approval ----------------

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var payslip = await _db.Payslips.FindAsync(id);
            if (payslip == null) return Json(new { success = false, message = "Payslip not found." });
            if (payslip.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending payslips can be approved." });

            var user = await _userManager.GetUserAsync(User);
            payslip.Status = VoucherStatus.Approved;
            payslip.ApprovedById = user?.Id;
            payslip.ApprovedByName = user?.FullName;
            payslip.ApprovedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Approval is what puts the salary, the advance recovery and the fixed
            // monthly deduction onto the employee's ledger (see LedgerService).
            return Json(new { success = true, message = $"Payslip {payslip.PayslipNumber} approved and posted to the employee ledger." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest payload)
        {
            var payslip = await _db.Payslips.FindAsync(payload.Id);
            if (payslip == null) return Json(new { success = false, message = "Payslip not found." });
            if (payslip.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending payslips can be rejected." });

            var user = await _userManager.GetUserAsync(User);
            payslip.Status = VoucherStatus.Rejected;
            payslip.ApprovedById = user?.Id;
            payslip.ApprovedByName = user?.FullName;
            payslip.ApprovedOn = DateTime.UtcNow;
            payslip.RejectionReason = payload.Reason;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Payslip {payslip.PayslipNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var payslip = await _db.Payslips.FindAsync(id);
            if (payslip == null) return Json(new { success = false, message = "Payslip not found." });
            if (payslip.Status == VoucherStatus.Approved)
                return Json(new { success = false, message = "Approved payslips cannot be deleted — they carry the advance balance forward." });

            _db.Payslips.Remove(payslip);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Payslip deleted." });
        }

        private async Task<Payslip?> LoadFullAsync(int id) =>
            await _db.Payslips
                .Include(p => p.Employee).ThenInclude(e => e!.DepartmentRef)
                .Include(p => p.Employee).ThenInclude(e => e!.ShiftTiming)
                .Include(p => p.PayRun)
                .FirstOrDefaultAsync(p => p.Id == id);

        private async Task PopulateDropdownsAsync(PayslipGenerateViewModel vm)
        {
            vm.Departments = await _db.Departments.Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentOption(d.Id, d.Name, d.Code))
                .ToListAsync();

            vm.Employees = await _db.Employees.Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                .ToListAsync();
        }
    }
}
