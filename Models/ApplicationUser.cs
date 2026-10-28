using Microsoft.AspNetCore.Identity;

namespace ERP.Identity.Models
{
    /// <summary>
    /// Extends the default Identity user with the fields an ERP HR/Accounts
    /// module typically needs. Keep this as the single source of truth for
    /// "who is this user" — sales/purchases/payroll modules built later can
    /// just reference the Id.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string? EmployeeCode { get; set; }

        public string? Designation { get; set; }

        public string? Department { get; set; }

        public string? Gender { get; set; }

        public string? BloodGroup { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime? JoiningDate { get; set; }

        public string? NationalId { get; set; } // CNIC / National ID / SSN

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? Bio { get; set; }

        /// <summary>Relative path under wwwroot, e.g. /uploads/avatars/xyz.png</summary>
        public string? ProfileImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginOn { get; set; }
    }
}
