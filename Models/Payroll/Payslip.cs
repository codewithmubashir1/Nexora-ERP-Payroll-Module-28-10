using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Payroll
{
    public class PayRun
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string RunNumber { get; set; } = string.Empty;

        public int Month { get; set; }
        public int Year { get; set; }

        public int TotalEmployees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGrossAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNetAmount { get; set; }

        public bool PayImmediately { get; set; }
        public int? PaymentAccountId { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
    }

    public class Payslip
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string PayslipNumber { get; set; } = string.Empty;

        public int? PayRunId { get; set; }
        public PayRun? PayRun { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HouseRentAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MedicalAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAllowances { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        public int WorkingDays { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal OvertimeHours { get; set; }

        /// <summary>Derived from the month's AttendanceRecord.LateByMinutes, converted to hours.</summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal LateHours { get; set; }

        /// <summary>OvertimeHours − LateHours.</summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal NetHours { get; set; }

        /// <summary>NetHours × (BasicSalary / 30 / Shift Hours).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimeAmount { get; set; }

        /// <summary>Outstanding advance balance carried in from before this payslip.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvancePreviousBalance { get; set; }

        /// <summary>New advance/loan disbursed and included in this period (from approved Loan/Advance requests not yet reflected in a prior payslip).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAdvanceGiven { get; set; }

        /// <summary>AdvancePreviousBalance + CurrentAdvanceGiven.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAdvance { get; set; }

        /// <summary>How much of TotalAdvance is recovered from this salary.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceDeducted { get; set; }

        /// <summary>TotalAdvance − AdvanceDeducted, carried forward to the next payslip.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceRemainingBalance { get; set; }

        /// <summary>Flat per-employee deduction each month (see OtherPayrollSettings.MonthlyFixedDeduction) — posted to the employee's ledger on approval.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal FixedMonthlyDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IncomeTaxDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal StatutoryDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LoanDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AbsentDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPay { get; set; }

        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
