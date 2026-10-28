using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ERP.Identity.Models.ViewModels
{
    public class CompanySettingsViewModel
    {
        [Required, Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        public string? Address { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        [Display(Name = "NTN Number")]
        [RegularExpression(@"^\d{7}-\d$", ErrorMessage = "NTN must be in the format 0000000-0")]
        public string? NTNNumber { get; set; }

        [Display(Name = "Sales Tax Registration Number")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{4}-\d{3}-\d{2}$", ErrorMessage = "STRN must be in the format 00-00-0000-000-00")]
        public string? SalesTaxRegistrationNumber { get; set; }

        public string? ExistingLogoPath { get; set; }
        public IFormFile? Logo { get; set; }

        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }

        [Display(Name = "Account Title")]
        public string? BankAccountTitle { get; set; }

        [Display(Name = "Account Number")]
        public string? BankAccountNumber { get; set; }
    }
}
