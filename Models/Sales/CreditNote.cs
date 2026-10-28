using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Sales
{
    public class CreditNote
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string CreditNoteNumber { get; set; } = string.Empty;

        public DateTime CreditNoteDate { get; set; } = DateTime.UtcNow;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        public int? DeliveryChallanId { get; set; }
        public DeliveryChallan? DeliveryChallan { get; set; }

        /// <summary>If set, approving this note also increases stock at this location (goods physically returned by customer).</summary>
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

        public ICollection<CreditNoteItem> Items { get; set; } = new List<CreditNoteItem>();
    }

    public class CreditNoteItem
    {
        public int Id { get; set; }

        public int CreditNoteId { get; set; }
        public CreditNote? CreditNote { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SalesTaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
