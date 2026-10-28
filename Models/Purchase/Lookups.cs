using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Purchase
{
    public class VendorCategory
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

        public ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
    }

    public class Vendor
    {
        public int Id { get; set; }

        /// <summary>System-generated, e.g. VND-000001. Never edited by the user.</summary>
        [Required, MaxLength(30)]
        public string VendorCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int VendorCategoryId { get; set; }
        public VendorCategory? VendorCategory { get; set; }

        [MaxLength(150)]
        public string? ContactPerson { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        /// <summary>Format: 0000000-0</summary>
        [MaxLength(15)]
        public string? NTNNumber { get; set; }

        /// <summary>Format: 00-00-0000-000-00</summary>
        [MaxLength(20)]
        public string? SalesTaxRegistrationNumber { get; set; }

        /// <summary>Standard payment terms in days (e.g. 30 = Net 30).</summary>
        public int PaymentTermsDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
