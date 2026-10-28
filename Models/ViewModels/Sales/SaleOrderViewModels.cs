using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Sales
{
    public class SaleOrderViewModel
    {
        public int Id { get; set; }

        [Display(Name = "SO Number")]
        public string? SONumber { get; set; }

        [Required, Display(Name = "SO Date")]
        public DateTime SODate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Expected Delivery")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Display(Name = "Customer PO Number(s)")]
        public string? CustomerPONumbers { get; set; }

        public string? Remarks { get; set; }

        public List<SaleOrderItemViewModel> Items { get; set; } = new() { new() };

        public List<CustomerOption> Customers { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class SaleOrderItemViewModel
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

    public class SaleOrderListItemViewModel
    {
        public int Id { get; set; }
        public string SONumber { get; set; } = string.Empty;
        public DateTime SODate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
