using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Sales
{
    public record DeliveryChallanOption(int Id, string ChallanNo);

    public class SalesInvoiceViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }

        [Required, Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; } = DateTime.Today;

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Payment Terms")]
        [Range(0, 3650)]
        public int? PaymentTermsDays { get; set; }

        [Required, Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Sale Order")]
        public int? SaleOrderId { get; set; }

        [Display(Name = "Delivery Challan")]
        public int? DeliveryChallanId { get; set; }

        [Display(Name = "Customer PO Number(s)")]
        public string? CustomerPONumbers { get; set; }

        public string? Remarks { get; set; }

        public List<SalesInvoiceItemViewModel> Items { get; set; } = new() { new() };

        public List<CustomerOption> Customers { get; set; } = new();
        public List<SaleOrderOption> SaleOrders { get; set; } = new();
        public List<DeliveryChallanOption> DeliveryChallans { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class SalesInvoiceItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal? TaxRate { get; set; }

        public string? Remarks { get; set; }
    }

    public class SalesInvoiceListItemViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
