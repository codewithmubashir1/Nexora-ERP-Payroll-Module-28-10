using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Purchase
{
    public record PurchaseOrderOption(int Id, string PONumber);

    public class InwardGatePassViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Gate Pass No.")]
        public string? GatePassNo { get; set; }

        [Required, Display(Name = "Gate Pass Date")]
        public DateTime GatePassDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Display(Name = "Purchase Order")]
        public int? PurchaseOrderId { get; set; }

        [Display(Name = "Vehicle No.")]
        public string? VehicleNo { get; set; }

        [Display(Name = "Driver Name")]
        public string? DriverName { get; set; }

        public string? Remarks { get; set; }

        public List<InwardGatePassItemViewModel> Items { get; set; } = new() { new() };

        public List<VendorOption> Vendors { get; set; } = new();
        public List<PurchaseOrderOption> PurchaseOrders { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class InwardGatePassItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal DeclaredQuantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? UnitCost { get; set; }

        public string? Remarks { get; set; }
    }

    public class InwardGatePassListItemViewModel
    {
        public int Id { get; set; }
        public string GatePassNo { get; set; } = string.Empty;
        public DateTime GatePassDate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string? VehicleNo { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
