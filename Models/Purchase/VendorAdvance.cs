using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Purchase
{
    public enum PaymentMethod
    {
        Cash = 0,
        BankTransfer = 1,
        Cheque = 2
    }

    public class VendorAdvance
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string AdvanceNumber { get; set; } = string.Empty;

        public DateTime AdvanceDate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public int? IssuedChequeId { get; set; }
        public IssuedCheque? IssuedCheque { get; set; }

        /// <summary>Running total of this advance already adjusted against invoice payments.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdjustedAmount { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal AvailableAmount => Amount - AdjustedAmount;
    }
}
