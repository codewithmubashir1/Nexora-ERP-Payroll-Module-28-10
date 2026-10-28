using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Purchase
{
    public class PurchaseInvoicePayment
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string PaymentNumber { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public int PurchaseInvoiceId { get; set; }
        public PurchaseInvoice? PurchaseInvoice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Withholding tax %, fixed dropdown values (0 to 5.5, step 0.5).</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? WithholdingTaxRate { get; set; }

        /// <summary>Amount × WithholdingTaxRate / 100 — deducted from what's actually disbursed to the vendor.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal WithholdingTaxAmount { get; set; }

        /// <summary>Amount − WithholdingTaxAmount — the actual cash/bank/cheque disbursement.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPaymentAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public int? IssuedChequeId { get; set; }
        public IssuedCheque? IssuedCheque { get; set; }

        /// <summary>Set when this payment is (partly) funded from a previously approved vendor advance.</summary>
        public int? VendorAdvanceId { get; set; }
        public VendorAdvance? VendorAdvance { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
