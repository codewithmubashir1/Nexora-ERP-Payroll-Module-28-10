using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Payroll;

namespace ERP.Identity.Models.ViewModels.Payroll
{
    /// <summary>Drives the shared _RequestList partial so the five request screens stay identical in behaviour.</summary>
    public class RequestListPageViewModel
    {
        public string Controller { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string NewButtonText { get; set; } = "New Request";

        /// <summary>Header for the category column (leave type, expense type, Loan/Advance…).</summary>
        public string CategoryLabel { get; set; } = "Type";

        /// <summary>Header for the free-text detail column.</summary>
        public string DetailLabel { get; set; } = "Details";

        /// <summary>Hidden for leave requests, which have no money attached.</summary>
        public bool ShowAmount { get; set; } = true;

        public bool CanApprove { get; set; }

        /// <summary>Optional note rendered above the table.</summary>
        public string? HelpText { get; set; }
    }

    /// <summary>Shared shape for every employee-request list (leave, loan/advance, mid salary, encashment, reimbursement).</summary>
    public class RequestListItemViewModel
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public DateTime CreatedOn { get; set; }

        /// <summary>Leave type / reimbursement type / Loan vs Advance — whatever best categorises this row.</summary>
        public string? Category { get; set; }

        /// <summary>Free-text summary column: "12 Jan – 15 Jan (4 days)", "3 days", etc.</summary>
        public string? Detail { get; set; }

        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LeaveRequestFormViewModel
    {
        public int Id { get; set; }
        public string? RequestNumber { get; set; }

        [Required(ErrorMessage = "Select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Select a leave type.")]
        [Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }

        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [MaxLength(500)]
        public string? Reason { get; set; }

        public List<PayrollEmployeeOption> Employees { get; set; } = new();
        public List<LeaveTypeOption> LeaveTypes { get; set; } = new();
    }

    public class LoanAdvanceRequestFormViewModel
    {
        public int Id { get; set; }
        public string? RequestNumber { get; set; }

        [Required(ErrorMessage = "Select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Display(Name = "Request Type")]
        public LoanAdvanceType RequestType { get; set; } = LoanAdvanceType.Advance;

        [Range(1, 999999999, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Display(Name = "Recover Over (months)")]
        [Range(1, 120, ErrorMessage = "Installments must be between 1 and 120 months.")]
        public int InstallmentMonths { get; set; } = 1;

        [MaxLength(500)]
        public string? Reason { get; set; }

        public List<PayrollEmployeeOption> Employees { get; set; } = new();
    }

    public class MidSalaryRequestFormViewModel
    {
        public int Id { get; set; }
        public string? RequestNumber { get; set; }

        [Required(ErrorMessage = "Select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Range(1, 999999999, ErrorMessage = "Amount must be greater than zero.")]
        [Display(Name = "Amount Requested")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public List<PayrollEmployeeOption> Employees { get; set; } = new();

        /// <summary>Half of the selected employee's basic salary — a conventional ceiling, surfaced as guidance rather than a hard rule.</summary>
        public decimal SuggestedMaximum { get; set; }
    }

    public class LeaveEncashmentRequestFormViewModel
    {
        public int Id { get; set; }
        public string? RequestNumber { get; set; }

        [Required(ErrorMessage = "Select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Select a leave type.")]
        [Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }

        [Range(1, 365, ErrorMessage = "Days to encash must be at least 1.")]
        [Display(Name = "Days to Encash")]
        public int DaysToEncash { get; set; } = 1;

        [MaxLength(500)]
        public string? Reason { get; set; }

        public List<PayrollEmployeeOption> Employees { get; set; } = new();

        /// <summary>Only leave types flagged IsEncashable are offered here.</summary>
        public List<LeaveTypeOption> LeaveTypes { get; set; } = new();
    }

    public class ReimbursementRequestFormViewModel
    {
        public int Id { get; set; }
        public string? RequestNumber { get; set; }

        [Required(ErrorMessage = "Select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Select an expense type.")]
        [Display(Name = "Expense Type")]
        public int ReimbursementTypeId { get; set; }

        [Range(1, 999999999, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Display(Name = "Expense Date")]
        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Receipt")]
        public IFormFile? Receipt { get; set; }

        public string? ExistingReceiptImagePath { get; set; }

        public List<PayrollEmployeeOption> Employees { get; set; } = new();
        public List<ReimbursementTypeOption> ReimbursementTypes { get; set; } = new();
    }
}
