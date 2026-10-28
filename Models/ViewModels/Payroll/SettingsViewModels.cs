using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Payroll;

namespace ERP.Identity.Models.ViewModels.Payroll
{
    /// <summary>Everything the Payroll Settings screen needs, loaded in one round trip.</summary>
    public class PayrollSettingsViewModel
    {
        public List<ReimbursementType> ReimbursementTypes { get; set; } = new();
        public List<LeaveType> LeaveTypes { get; set; } = new();
        public List<Holiday> Holidays { get; set; } = new();
        public List<StatutoryDeduction> StatutoryDeductions { get; set; } = new();
        public List<TaxSlab> TaxSlabs { get; set; } = new();
        public List<AttendancePolicy> Policies { get; set; } = new();
        public List<AttendanceDevice> Devices { get; set; } = new();

        public OtherPayrollSettings Other { get; set; } = new();
        public MachineIntegrationSetting MachineIntegration { get; set; } = new();

        /// <summary>Which section to open on load — set from the ?tab= query string after navigation or save.</summary>
        public string ActiveTab { get; set; } = "reimbursement";

        public int HolidayYear { get; set; } = DateTime.Today.Year;
    }

    public class SalaryIncrementFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Display(Name = "Effective Date")]
        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        [Display(Name = "Current Basic Salary")]
        public decimal OldBasicSalary { get; set; }

        [Range(0, 999999999, ErrorMessage = "New basic salary cannot be negative.")]
        [Display(Name = "New Basic Salary")]
        public decimal NewBasicSalary { get; set; }

        [MaxLength(300)]
        public string? Reason { get; set; }

        public List<PayrollEmployeeOption> Employees { get; set; } = new();
    }

    public class SalaryIncrementListItemViewModel
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public decimal OldBasicSalary { get; set; }
        public decimal NewBasicSalary { get; set; }
        public decimal IncreaseAmount { get; set; }
        public decimal IncreasePercent { get; set; }
        public string? Reason { get; set; }
        public string? ApprovedByName { get; set; }
    }

    // ---------------- Tax calculator ----------------

    public class TaxSlabBreakdownRow
    {
        public string Range { get; set; } = string.Empty;
        public decimal RatePercent { get; set; }
        public decimal TaxableInThisSlab { get; set; }
        public decimal TaxOnThisSlab { get; set; }
        public bool IsApplicableSlab { get; set; }
    }

    public class TaxCalculatorViewModel
    {
        [Display(Name = "Employee (optional)")]
        public int? EmployeeId { get; set; }

        [Display(Name = "Monthly Gross Salary")]
        [Range(0, 999999999, ErrorMessage = "Monthly salary cannot be negative.")]
        public decimal MonthlySalary { get; set; }

        [Display(Name = "Annual Bonus / Other Taxable Income")]
        [Range(0, 999999999)]
        public decimal OtherAnnualIncome { get; set; }

        [Display(Name = "Annual Tax Credits / Deductible Allowances")]
        [Range(0, 999999999)]
        public decimal AnnualDeductions { get; set; }

        [Display(Name = "Apply statutory deductions (EOBI, PF, …)")]
        public bool ApplyStatutory { get; set; } = true;

        public bool HasResult { get; set; }

        public decimal AnnualGross { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal AnnualTax { get; set; }
        public decimal MonthlyTax { get; set; }
        public decimal EffectiveRate { get; set; }
        public decimal StatutoryMonthly { get; set; }
        public decimal MonthlyTakeHome { get; set; }

        public List<TaxSlabBreakdownRow> Breakdown { get; set; } = new();
        public List<StatutoryDeduction> AppliedStatutory { get; set; } = new();
        public List<PayrollEmployeeOption> Employees { get; set; } = new();
    }
}
