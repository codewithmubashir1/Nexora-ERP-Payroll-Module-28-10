using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.ViewModels.Accounts
{
    public class EmployeeFormViewModel
    {
        public int Id { get; set; }

        public string? EmployeeCode { get; set; }

        [Required, Display(Name = "Employee Name")]
        public string Name { get; set; } = string.Empty;

        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Monthly Salary")]
        public decimal? MonthlySalary { get; set; }

        [Display(Name = "Joining Date")]
        public DateTime? JoiningDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public record EmployeeOption(int Id, string Name, string EmployeeCode);

    public class EmployeeListItemViewModel
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
    }
}
