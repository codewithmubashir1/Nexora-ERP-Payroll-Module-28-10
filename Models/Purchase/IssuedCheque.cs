using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Purchase
{
    /// <summary>Post-approval lifecycle of a cheque, tracked separately from the approval status.</summary>
    public enum ChequeClearanceStatus
    {
        Outstanding = 0,
        Cleared = 1,
        Bounced = 2,
        Cancelled = 3
    }

    public class IssuedCheque
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string ChequeReferenceNo { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ChequeNumber { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string BankName { get; set; } = string.Empty;

        public DateTime ChequeDate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public ChequeClearanceStatus ClearanceStatus { get; set; } = ChequeClearanceStatus.Outstanding;

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
