using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.ViewModels.Payroll
{
    /// <summary>
    /// The full result of running the salary formulas for one employee/month.
    /// Produced by IPayrollService.ComputeAsync and used to pre-fill the payslip
    /// form, to build pay runs, and to render the salary sheet.
    /// </summary>
    public class SalaryComputationViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? Designation { get; set; }
        public string? ShiftName { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal HouseRentAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowances { get; set; }

        /// <summary>Length of the employee's assigned shift, used as the divisor in the overtime rate.</summary>
        public decimal ShiftHours { get; set; }

        /// <summary>Per-hour rate: BasicSalary / 30 / ShiftHours.</summary>
        public decimal HourlyRate { get; set; }

        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }

        public decimal OvertimeHours { get; set; }
        public decimal LateHours { get; set; }

        /// <summary>OvertimeHours − LateHours (may be negative if the employee was late more than they worked over).</summary>
        public decimal NetHours { get; set; }

        /// <summary>NetHours × HourlyRate.</summary>
        public decimal OvertimeAmount { get; set; }

        /// <summary>BasicSalary / 30 × WorkingDays — the earned portion of basic pay.</summary>
        public decimal EarnedBasic { get; set; }

        public decimal AdvancePreviousBalance { get; set; }
        public decimal CurrentAdvanceGiven { get; set; }
        public decimal TotalAdvance { get; set; }
        public decimal AdvanceDeducted { get; set; }
        public decimal AdvanceRemainingBalance { get; set; }

        /// <summary>Flat monthly deduction (Rs 300 by default) that is credited to the employee's ledger.</summary>
        public decimal FixedMonthlyDeduction { get; set; }

        public decimal IncomeTaxDeduction { get; set; }
        public decimal StatutoryDeductions { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal AbsentDeduction { get; set; }

        public decimal TotalDeductions { get; set; }

        /// <summary>EarnedBasic (+ OvertimeAmount when IncludeOvertimeInNetPay) − all deductions.</summary>
        public decimal NetPay { get; set; }

        public bool IncludeOvertimeInNetPay { get; set; }

        /// <summary>True when a payslip for this employee/month already exists.</summary>
        public bool AlreadyGenerated { get; set; }
        public int? ExistingPayslipId { get; set; }

        /// <summary>Non-blocking notes surfaced to the user, e.g. "No attendance recorded — 30 days assumed".</summary>
        public List<string> Notes { get; set; } = new();
    }

    public class PayslipGenerateViewModel
    {
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Today.Year;

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Employee")]
        public int? EmployeeId { get; set; }

        [Display(Name = "Include overtime amount in net pay")]
        public bool IncludeOvertimeInNetPay { get; set; }

        public List<SalaryComputationViewModel> Preview { get; set; } = new();
        public List<DepartmentOption> Departments { get; set; } = new();
        public List<PayrollEmployeeOption> Employees { get; set; } = new();

        public bool HasPreview { get; set; }
    }

    public class PayslipListItemViewModel
    {
        public int Id { get; set; }
        public string PayslipNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string Period { get; set; } = string.Empty;
        public string? PayRunNumber { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SalarySheetRowViewModel
    {
        public int PayslipId { get; set; }
        public string PayslipNumber { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? Designation { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }

        public decimal BasicSalary { get; set; }
        public int WorkingDays { get; set; }
        public decimal AttendanceHours { get; set; }
        public decimal EarnedBasic { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal LateHours { get; set; }
        public decimal NetHours { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal AdvancePreviousBalance { get; set; }
        public decimal CurrentAdvanceGiven { get; set; }
        public decimal TotalAdvance { get; set; }
        public decimal AdvanceDeducted { get; set; }
        public decimal AdvanceRemainingBalance { get; set; }
        public decimal FixedMonthlyDeduction { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SalarySheetViewModel
    {
        public int Month { get; set; } = DateTime.Today.Month;
        public int Year { get; set; } = DateTime.Today.Year;
        public int? DepartmentId { get; set; }

        public List<SalarySheetRowViewModel> Rows { get; set; } = new();
        public List<DepartmentOption> Departments { get; set; } = new();

        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");

        public decimal TotalEarnedBasic => Rows.Sum(r => r.EarnedBasic);
        public decimal TotalAttendanceHours => Rows.Sum(r => r.AttendanceHours);
        public decimal TotalOvertimeAmount => Rows.Sum(r => r.OvertimeAmount);
        public decimal TotalAdvanceDeducted => Rows.Sum(r => r.AdvanceDeducted);
        public decimal TotalFixedDeduction => Rows.Sum(r => r.FixedMonthlyDeduction);
        public decimal TotalDeductionsSum => Rows.Sum(r => r.TotalDeductions);
        public decimal TotalNetPay => Rows.Sum(r => r.NetPay);
    }

    public class PayRunListItemViewModel
    {
        public int Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public class PayRunCreateViewModel
    {
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Today.Year;

        [Display(Name = "Department (optional)")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Include overtime amount in net pay")]
        public bool IncludeOvertimeInNetPay { get; set; }

        [Display(Name = "Pay employees immediately on approval")]
        public bool PayImmediately { get; set; }

        [Display(Name = "Pay from")]
        public int? PaymentAccountId { get; set; }

        public List<SalaryComputationViewModel> Preview { get; set; } = new();
        public List<DepartmentOption> Departments { get; set; } = new();
        public List<ERP.Identity.Models.ViewModels.Accounts.AccountOption> PaymentAccounts { get; set; } = new();
        public bool HasPreview { get; set; }

        public decimal TotalGross => Preview.Sum(p => p.EarnedBasic + (IncludeOvertimeInNetPay ? p.OvertimeAmount : 0));
        public decimal TotalDeductions => Preview.Sum(p => p.TotalDeductions);
        public decimal TotalNet => Preview.Sum(p => p.NetPay);
    }
}
