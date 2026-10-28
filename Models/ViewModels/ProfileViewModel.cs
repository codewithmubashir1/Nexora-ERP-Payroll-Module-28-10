using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ERP.Identity.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone, Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

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

        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }

        [Display(Name = "About Me")]
        [DataType(DataType.MultilineText)]
        public string? Bio { get; set; }

        public string? ProfileImagePath { get; set; }

        public IFormFile? ProfileImage { get; set; }

        public string? CurrentRole { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password), Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "Confirm New Password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
