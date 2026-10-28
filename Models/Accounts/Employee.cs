using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Payroll;

namespace ERP.Identity.Models.Accounts
{
    public class Employee
    {
        public int Id { get; set; }

        /// <summary>System-generated, e.g. EMP-000001.</summary>
        [Required, MaxLength(30)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Legacy free-text department, kept for backward compatibility. New records use DepartmentId.</summary>
        [MaxLength(100)]
        public string? Department { get; set; }

        public int? DepartmentId { get; set; }
        public Department? DepartmentRef { get; set; }

        public int? ShiftTimingId { get; set; }
        public ShiftTiming? ShiftTiming { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        /// <summary>Format: 00000-0000000-0</summary>
        [MaxLength(15)]
        public string? CNICNumber { get; set; }

        [MaxLength(3)]
        public string? BloodGroup { get; set; }

        /// <summary>Dial code only, e.g. "+92". Stored separately from Phone so the country picker can be re-shown when editing.</summary>
        [MaxLength(6)]
        public string? ContactCountryCode { get; set; } = "+92";

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ZipCode { get; set; }

        public string? ProfileImagePath { get; set; }

        /// <summary>Kept for backward compatibility / quick reference — equals BasicSalary + all allowances.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlySalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HouseRentAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MedicalAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAllowances { get; set; }

        [MaxLength(150)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(20)]
        public string? TaxNumber { get; set; }

        [MaxLength(30)]
        public string? EOBINumber { get; set; }

        public DateTime? JoiningDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
