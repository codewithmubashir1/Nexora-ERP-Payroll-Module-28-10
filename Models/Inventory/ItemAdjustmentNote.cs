using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Inventory
{
    public enum AdjustmentType
    {
        Increase = 0,
        Decrease = 1
    }

    public class ItemAdjustmentNote
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string AdjustmentNo { get; set; } = string.Empty;

        public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;

        public int LocationId { get; set; }
        public Location? Location { get; set; }

        /// <summary>Stock Count, Damaged, Expired, Lost, Found, Other...</summary>
        [MaxLength(100)]
        public string? Reason { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;

        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<ItemAdjustmentNoteItem> Items { get; set; } = new List<ItemAdjustmentNoteItem>();
    }

    public class ItemAdjustmentNoteItem
    {
        public int Id { get; set; }

        public int ItemAdjustmentNoteId { get; set; }
        public ItemAdjustmentNote? ItemAdjustmentNote { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public AdjustmentType AdjustmentType { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
