using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.ViewModels.Inventory
{
    public class ProductCategoryViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        public string? Code { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public int ProductCount { get; set; }
    }

    public class LocationViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Location Name")]
        public string Name { get; set; } = string.Empty;

        public string? Code { get; set; }

        public string? Address { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
