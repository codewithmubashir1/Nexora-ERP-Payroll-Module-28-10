using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ERP.Identity.Models.ViewModels
{
    /// <summary>Used for both "create user" and "edit user" (admin Users module).</summary>
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Employee Code")]
        public string? EmployeeCode { get; set; }

        public string? Designation { get; set; }

        public string? Department { get; set; }

        public string? Gender { get; set; }

        [Display(Name = "Blood Group"), MaxLength(3)]
        public string? BloodGroup { get; set; }

        [DataType(DataType.Date), Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [DataType(DataType.Date), Display(Name = "Joining Date")]
        public DateTime? JoiningDate { get; set; }

        [Display(Name = "National ID / CNIC")]
        public string? NationalId { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Role")]
        public string? SelectedRole { get; set; }

        public List<string> AvailableRoles { get; set; } = AppRoles.All.ToList();

        public IFormFile? ProfileImage { get; set; }

        public string? ExistingProfileImagePath { get; set; }
    }
}
