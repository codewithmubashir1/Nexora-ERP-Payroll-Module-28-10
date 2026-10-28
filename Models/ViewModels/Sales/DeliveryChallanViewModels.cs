using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Sales
{
    public record SaleOrderOption(int Id, string SONumber);

    public class DeliveryChallanViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Challan No.")]
        public string? ChallanNo { get; set; }

        [Required, Display(Name = "Challan Date")]
        public DateTime ChallanDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Sale Order")]
        public int? SaleOrderId { get; set; }

        [Required, Display(Name = "Shipping Location")]
        public int LocationId { get; set; }

        [Display(Name = "Vehicle No.")]
        public string? VehicleNo { get; set; }

        [Display(Name = "Driver Name")]
        public string? DriverName { get; set; }

        public string? Remarks { get; set; }

        public List<DeliveryChallanItemViewModel> Items { get; set; } = new() { new() };

        public List<CustomerOption> Customers { get; set; } = new();
        public List<SaleOrderOption> SaleOrders { get; set; } = new();
        public List<LocationOption> Locations { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class DeliveryChallanItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? UnitPrice { get; set; }

        public string? Remarks { get; set; }
    }

    public class DeliveryChallanListItemViewModel
    {
        public int Id { get; set; }
        public string ChallanNo { get; set; } = string.Empty;
        public DateTime ChallanDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
