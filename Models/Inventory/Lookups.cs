using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.Inventory
{
    /// <summary>Product categorization (e.g. Raw Material, Finished Goods, Consumables).</summary>
    public class ProductCategory
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Code { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>A physical stock location — warehouse, store, or outlet.</summary>
    public class Location
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Code { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Unit of measure used on the Product dropdown (Pcs, Kg, Litre, Box...).</summary>
    public class UnitOfMeasure
    {
        public int Id { get; set; }

        [Required, MaxLength(60)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string ShortName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
