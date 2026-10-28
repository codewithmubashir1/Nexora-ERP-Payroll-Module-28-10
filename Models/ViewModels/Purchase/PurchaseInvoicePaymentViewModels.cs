using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Purchase;

namespace ERP.Identity.Models.ViewModels.Purchase
{
    public record InvoiceOption(int Id, string InvoiceNumber, decimal Balance, int VendorId);
    public record ChequeOption(int Id, string ChequeReferenceNo, string ChequeNumber);
    public record AdvanceOption(int Id, string AdvanceNumber, decimal AvailableAmount);

    public class PurchaseInvoicePaymentViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Payment Number")]
        public string? PaymentNumber { get; set; }

        [Required, Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Required, Display(Name = "Invoice")]
        public int PurchaseInvoiceId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Display(Name = "Withholding Tax")]
        [Range(0, 100)]
        public decimal? WithholdingTaxRate { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "Issued Cheque")]
        public int? IssuedChequeId { get; set; }

        [Display(Name = "Adjust Against Advance")]
        public int? VendorAdvanceId { get; set; }

        public string? Remarks { get; set; }

        public List<VendorOption> Vendors { get; set; } = new();
        public List<InvoiceOption> Invoices { get; set; } = new();
        public List<ChequeOption> Cheques { get; set; } = new();
        public List<AdvanceOption> Advances { get; set; } = new();
    }

    public class PurchaseInvoicePaymentListItemViewModel
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
