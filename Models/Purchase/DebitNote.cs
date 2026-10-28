using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Purchase
{
    public class DebitNote
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string DebitNoteNumber { get; set; } = string.Empty;

        public DateTime DebitNoteDate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public int? PurchaseInvoiceId { get; set; }
        public PurchaseInvoice? PurchaseInvoice { get; set; }

        public int? GoodsReceiptNoteId { get; set; }
        public GoodsReceiptNote? GoodsReceiptNote { get; set; }

        /// <summary>If set, approving this note also reduces stock at this location (goods physically returned to vendor).</summary>
        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        [MaxLength(150)]
        public string? Reason { get; set; }

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

        public ICollection<DebitNoteItem> Items { get; set; } = new List<DebitNoteItem>();
    }

    public class DebitNoteItem
    {
        public int Id { get; set; }

        public int DebitNoteId { get; set; }
        public DebitNote? DebitNote { get; set; }

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
