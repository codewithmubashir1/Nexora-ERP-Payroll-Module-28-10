using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Payroll
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int LeaveTypeId { get; set; }
        public LeaveType? LeaveType { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int TotalDays { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public enum LoanAdvanceType
    {
        Loan = 0,
        Advance = 1
    }

    public class LoanAdvanceRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public LoanAdvanceType RequestType { get; set; } = LoanAdvanceType.Advance;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Over how many months this will be deducted from salary — 1 for a simple advance settled next payslip.</summary>
        public int InstallmentMonths { get; set; } = 1;

        [MaxLength(500)]
        public string? Reason { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public class MidSalaryRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public class LeaveEncashmentRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int LeaveTypeId { get; set; }
        public LeaveType? LeaveType { get; set; }

        public int DaysToEncash { get; set; }

        /// <summary>Computed at submission time from (BasicSalary / 30) × DaysToEncash — kept as a stored value so later salary changes don't retroactively alter an already-submitted request.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public class ReimbursementRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int ReimbursementTypeId { get; set; }
        public ReimbursementType? ReimbursementType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? ReceiptImagePath { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
