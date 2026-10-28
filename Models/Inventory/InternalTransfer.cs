using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Inventory
{
    public class InternalTransfer
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string TransferNo { get; set; } = string.Empty;

        public DateTime TransferDate { get; set; } = DateTime.UtcNow;

        public int FromLocationId { get; set; }
        public Location? FromLocation { get; set; }

        public int ToLocationId { get; set; }
        public Location? ToLocation { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;

        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<InternalTransferItem> Items { get; set; } = new List<InternalTransferItem>();
    }

    public class InternalTransferItem
    {
        public int Id { get; set; }

        public int InternalTransferId { get; set; }
        public InternalTransfer? InternalTransfer { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
