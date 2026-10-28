using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.ViewModels.Purchase
{
    public class VendorCategoryViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        public string? Code { get; set; }
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public int VendorCount { get; set; }
    }

    public class VendorFormViewModel
    {
        public int Id { get; set; }

        public string? VendorCode { get; set; }

        [Required, Display(Name = "Vendor Name")]
        public string Name { get; set; } = string.Empty;

        [Required, Display(Name = "Category")]
        public int VendorCategoryId { get; set; }

        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        [Display(Name = "NTN Number")]
        [RegularExpression(@"^\d{7}-\d$", ErrorMessage = "NTN must be in the format 0000000-0")]
        public string? NTNNumber { get; set; }

        [Display(Name = "Sales Tax Registration Number")]
        [RegularExpression(@"^\d{2}-\d{2}-\d{4}-\d{3}-\d{2}$", ErrorMessage = "STRN must be in the format 00-00-0000-000-00")]
        public string? SalesTaxRegistrationNumber { get; set; }

        [Display(Name = "Payment Terms (days)")]
        [Range(0, 365)]
        public int PaymentTermsDays { get; set; }

        [Display(Name = "Opening Balance")]
        public decimal OpeningBalance { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<VendorCategoryOption> Categories { get; set; } = new();
    }

    public record VendorCategoryOption(int Id, string Name);
    public record VendorOption(int Id, string Name, string VendorCode);

    public class VendorListItemViewModel
    {
        public int Id { get; set; }
        public string VendorCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int PaymentTermsDays { get; set; }
        public bool IsActive { get; set; }
    }
}
