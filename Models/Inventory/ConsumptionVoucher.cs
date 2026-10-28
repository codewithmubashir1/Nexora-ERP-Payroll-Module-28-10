using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Inventory
{
    public class ConsumptionVoucher
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string VoucherNo { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        public int LocationId { get; set; }
        public Location? Location { get; set; }

        [MaxLength(150)]
        public string? Department { get; set; }

        [MaxLength(150)]
        public string? IssuedTo { get; set; }

        [MaxLength(200)]
        public string? Purpose { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;

        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<ConsumptionVoucherItem> Items { get; set; } = new List<ConsumptionVoucherItem>();
    }

    public class ConsumptionVoucherItem
    {
        public int Id { get; set; }

        public int ConsumptionVoucherId { get; set; }
        public ConsumptionVoucher? ConsumptionVoucher { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
