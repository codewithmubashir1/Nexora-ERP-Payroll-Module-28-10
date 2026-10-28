using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Purchase
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string PONumber { get; set; } = string.Empty;

        public DateTime PODate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }

    public class PurchaseOrderItem
    {
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>Sales tax %, fixed dropdown values (16/17/18/19).</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? SalesTaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
