using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ERP.Identity.Models.ViewModels.Inventory
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        /// <summary>Read-only once assigned; shown on the form but generated server-side.</summary>
        [Display(Name = "SKU")]
        public string? Sku { get; set; }

        [Display(Name = "Product Code")]
        public string? ProductCode { get; set; }

        [Display(Name = "HS Code")]
        public string? HsCode { get; set; }

        [Required, Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required, Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required, Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required, Display(Name = "Cost Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost price cannot be negative")]
        public decimal CostPrice { get; set; }

        [Required, Display(Name = "Sale Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Sale price cannot be negative")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Tax Rate (%)")]
        [Range(0, 100)]
        public decimal? TaxRate { get; set; }

        [Display(Name = "Reorder Level")]
        [Range(0, double.MaxValue)]
        public decimal ReorderLevel { get; set; }

        [Display(Name = "Opening Stock")]
        [Range(0, double.MaxValue)]
        public decimal OpeningStock { get; set; }

        [Display(Name = "Default Location")]
        public int? DefaultLocationId { get; set; }

        [Display(Name = "Saleable")]
        public bool IsSaleable { get; set; } = true;

        [Display(Name = "Purchasable")]
        public bool IsPurchasable { get; set; } = true;

        [Display(Name = "Service")]
        public bool IsService { get; set; }

        [Display(Name = "Consumable")]
        public bool IsConsumable { get; set; }

        [Display(Name = "Packing Unit")]
        public string? PackingUnitName { get; set; }

        [Display(Name = "Packing Quantity")]
        [Range(0, double.MaxValue)]
        public decimal? PackingQuantity { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public IFormFile? ProductImage { get; set; }

        public string? ExistingProductImagePath { get; set; }

        public List<ProductCategoryOption> Categories { get; set; } = new();
        public List<UnitOption> Units { get; set; } = new();
        public List<LocationOption> Locations { get; set; } = new();
    }

    public record ProductCategoryOption(int Id, string Name);
    public record UnitOption(int Id, string Name, string ShortName);
    public record LocationOption(int Id, string Name);

    public class ProductListItemViewModel
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? ProductCode { get; set; }
        public string? HsCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string UnitShortName { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal CurrentStock { get; set; }
        public string StockStatus { get; set; } = "Normal"; // Normal | Low | Out
        public bool IsActive { get; set; }
        public bool IsDuplicate { get; set; }
        public string? ProductImagePath { get; set; }
    }
}
