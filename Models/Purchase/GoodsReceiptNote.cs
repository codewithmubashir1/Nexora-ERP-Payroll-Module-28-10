using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Purchase
{
    public class GoodsReceiptNote
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string GRNNumber { get; set; } = string.Empty;

        public DateTime GRNDate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int? InwardGatePassId { get; set; }
        public InwardGatePass? InwardGatePass { get; set; }

        /// <summary>Warehouse/location the goods are received into — stock posts here on approval.</summary>
        public int LocationId { get; set; }
        public Location? Location { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<GoodsReceiptNoteItem> Items { get; set; } = new List<GoodsReceiptNoteItem>();
    }

    public class GoodsReceiptNoteItem
    {
        public int Id { get; set; }

        public int GoodsReceiptNoteId { get; set; }
        public GoodsReceiptNote? GoodsReceiptNote { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Quantity expected — copied from the linked PO/Gate Pass line when one is selected. Informational only.</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal DeclaredQuantity { get; set; }

        /// <summary>Quantity that passed inspection — this is what posts to the stock ledger and is billed.</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal AcceptedQuantity { get; set; }

        /// <summary>Quantity that failed inspection — informational, does not affect stock. Auto-calculated client-side as Declared − Accepted, but stored as entered.</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal RejectedQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        /// <summary>AcceptedQuantity × UnitCost — only accepted goods are billed.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
