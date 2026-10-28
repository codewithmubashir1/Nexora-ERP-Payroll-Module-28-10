using ERP.Identity.Data;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>
    /// Works out annual income tax from the slabs configured in Payroll Settings and
    /// shows the per-slab working, so a payroll officer can explain a deduction rather
    /// than just quote it.
    /// </summary>
    [Authorize]
    [Route("Payroll/TaxCalculator")]
    public class TaxCalculatorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;

        public TaxCalculatorController(ApplicationDbContext db, IPayrollService payrollService)
        {
            _db = db;
            _payrollService = payrollService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var vm = new TaxCalculatorViewModel();
            await PopulateAsync(vm);
            return View(vm);
        }

        [HttpPost("Calculate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(TaxCalculatorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAsync(model);
                return View("Index", model);
            }

            model.AnnualGross = Math.Round((model.MonthlySalary * 12m) + model.OtherAnnualIncome, 2);
            model.TaxableIncome = Math.Max(0, Math.Round(model.AnnualGross - model.AnnualDeductions, 2));

            var (annualTax, breakdown) = await _payrollService.CalculateAnnualTaxAsync(model.TaxableIncome);

            model.AnnualTax = annualTax;
            model.MonthlyTax = Math.Round(annualTax / 12m, 2);
            model.Breakdown = breakdown;
            model.EffectiveRate = model.TaxableIncome > 0
                ? Math.Round(annualTax / model.TaxableIncome * 100m, 2)
                : 0m;

            if (model.ApplyStatutory)
            {
                model.AppliedStatutory = await _db.StatutoryDeductions.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();

                foreach (var deduction in model.AppliedStatutory)
                {
                    // A ceiling like EOBI's caps the base the percentage applies to.
                    var applicableBase = deduction.MaxApplicableSalary.HasValue
                        ? Math.Min(model.MonthlySalary, deduction.MaxApplicableSalary.Value)
                        : model.MonthlySalary;

                    model.StatutoryMonthly += Math.Round(applicableBase * deduction.RatePercent / 100m, 2);
                }
            }

            model.MonthlyTakeHome = Math.Round(model.MonthlySalary - model.MonthlyTax - model.StatutoryMonthly, 2);
            model.HasResult = true;

            await PopulateAsync(model);
            return View("Index", model);
        }

        /// <summary>Lets the screen refill itself when an employee is picked.</summary>
        [HttpGet("GetEmployeeSalary/{id:int}")]
        public async Task<IActionResult> GetEmployeeSalary(int id)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var monthly = employee.BasicSalary + employee.HouseRentAllowance
                + employee.MedicalAllowance + employee.OtherAllowances;

            return Json(new { monthlySalary = monthly, basicSalary = employee.BasicSalary, name = employee.Name });
        }

        private async Task PopulateAsync(TaxCalculatorViewModel vm)
        {
            vm.Employees = await _db.Employees.Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                .ToListAsync();
        }
    }
}
