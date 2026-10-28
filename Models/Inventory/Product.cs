using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Inventory
{
    public class Product
    {
        public int Id { get; set; }

        /// <summary>System-generated, e.g. SKU-000001. Never edited by the user.</summary>
        [Required, MaxLength(30)]
        public string Sku { get; set; } = string.Empty;

        /// <summary>User-entered internal code, e.g. RM-STEEL-01.</summary>
        [MaxLength(50)]
        public string? ProductCode { get; set; }

        /// <summary>Harmonized System code, used for customs/tax classification.</summary>
        [MaxLength(20)]
        public string? HsCode { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public ProductCategory? Category { get; set; }

        public int UnitId { get; set; }
        public UnitOfMeasure? Unit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        /// <summary>Tax / GST percentage applied on sale.</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxRate { get; set; }

        /// <summary>Minimum stock level before a re-order alert would trigger.</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal ReorderLevel { get; set; }

        /// <summary>Quantity posted to the stock ledger the moment the product is created.</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal OpeningStock { get; set; }

        public int? DefaultLocationId { get; set; }
        public Location? DefaultLocation { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? ProductImagePath { get; set; }

        /// <summary>Which contexts this product should appear in. Flags — a product can be more than one type.</summary>
        public ProductType ProductType { get; set; } = ProductType.Saleable | ProductType.Purchasable;

        /// <summary>Standard packing unit name, e.g. "Box", "Bin", "Bag", "Packet". Null = no standard packing defined.</summary>
        [MaxLength(30)]
        public string? PackingUnitName { get; set; }

        /// <summary>How many base units make up one packing unit, e.g. 24 pieces per Box.</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? PackingQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }

    [Flags]
    public enum ProductType
    {
        None = 0,
        Saleable = 1,
        Purchasable = 2,
        Service = 4,
        Consumable = 8
    }
}
