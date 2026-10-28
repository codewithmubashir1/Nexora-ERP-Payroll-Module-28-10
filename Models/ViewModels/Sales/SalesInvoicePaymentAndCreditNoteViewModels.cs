using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Purchase;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Sales
{
    public record SalesInvoiceOption(int Id, string InvoiceNumber, decimal Balance, int CustomerId);

    public class SalesInvoicePaymentViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Payment Number")]
        public string? PaymentNumber { get; set; }

        [Required, Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required, Display(Name = "Invoice")]
        public int SalesInvoiceId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Display(Name = "Withholding Tax")]
        [Range(0, 100)]
        public decimal? WithholdingTaxRate { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "Cheque Number")]
        public string? ChequeNumber { get; set; }

        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }

        public string? Remarks { get; set; }

        public List<CustomerOption> Customers { get; set; } = new();
        public List<SalesInvoiceOption> Invoices { get; set; } = new();
    }

    public class SalesInvoicePaymentListItemViewModel
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CreditNoteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Credit Note No.")]
        public string? CreditNoteNumber { get; set; }

        [Required, Display(Name = "Credit Note Date")]
        public DateTime CreditNoteDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Sales Invoice")]
        public int? SalesInvoiceId { get; set; }

        [Display(Name = "Delivery Challan")]
        public int? DeliveryChallanId { get; set; }

        [Display(Name = "Return Location")]
        public int? LocationId { get; set; }

        public string? Reason { get; set; }
        public string? Remarks { get; set; }

        public List<CreditNoteItemViewModel> Items { get; set; } = new() { new() };

        public List<CustomerOption> Customers { get; set; } = new();
        public List<SalesInvoiceOption> Invoices { get; set; } = new();
        public List<DeliveryChallanOption> DeliveryChallans { get; set; } = new();
        public List<LocationOption> Locations { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class CreditNoteItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal? SalesTaxRate { get; set; }

        public string? Remarks { get; set; }
    }

    public class CreditNoteListItemViewModel
    {
        public int Id { get; set; }
        public string CreditNoteNumber { get; set; } = string.Empty;
        public DateTime CreditNoteDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
