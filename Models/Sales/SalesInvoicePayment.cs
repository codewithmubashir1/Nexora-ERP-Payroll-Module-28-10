using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Purchase;

namespace ERP.Identity.Models.Sales
{
    public class SalesInvoicePayment
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string PaymentNumber { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Withholding tax % the customer applies when paying us, fixed dropdown values (0 to 5.5, step 0.5).</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? WithholdingTaxRate { get; set; }

        /// <summary>Amount × WithholdingTaxRate / 100 — retained by the customer, to be adjusted in our tax filings.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal WithholdingTaxAmount { get; set; }

        /// <summary>Amount − WithholdingTaxAmount — the actual cash/bank/cheque received.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetReceivedAmount { get; set; }

        /// <summary>Reuses the Cash/BankTransfer/Cheque enum already defined for the Purchase module.</summary>
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [MaxLength(50)]
        public string? ChequeNumber { get; set; }

        [MaxLength(150)]
        public string? BankName { get; set; }

        /// <summary>Only meaningful when PaymentMethod is Cheque — feeds the unified Cheque Register alongside Purchase's Issued Cheques.</summary>
        public ChequeClearanceStatus? ClearanceStatus { get; set; }

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
