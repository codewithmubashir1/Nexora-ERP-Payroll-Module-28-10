using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.Inventory
{
    /// <summary>
    /// Maps a product to the name/code a specific customer knows it by. Useful for
    /// B2B catalogs where every customer has their own part numbers. Populated via
    /// the "Import Customer-wise Product Names" tool on the Products page.
    /// CustomerName is free text for now — once the Sales module adds a real
    /// Customer entity, swap this for a CustomerId foreign key.
    /// </summary>
    public class CustomerProductAlias
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [MaxLength(200)]
        public string? CustomerProductName { get; set; }

        [MaxLength(100)]
        public string? CustomerProductCode { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
