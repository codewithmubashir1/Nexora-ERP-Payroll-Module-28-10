using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Inventory
{
    /// <summary>
    /// Audit trail of cost/sale price changes on a product. A row is written
    /// automatically whenever ProductsController.Save() detects either price
    /// field differs from what's currently stored.
    /// </summary>
    public class ProductPriceHistory
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OldCostPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewCostPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OldSalePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewSalePrice { get; set; }

        public string? ChangedById { get; set; }
        public string? ChangedByName { get; set; }

        public DateTime ChangedOn { get; set; } = DateTime.UtcNow;
    }
}
