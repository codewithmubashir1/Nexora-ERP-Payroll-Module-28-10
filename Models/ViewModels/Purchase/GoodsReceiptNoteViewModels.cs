using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Purchase
{
    public record GatePassOption(int Id, string GatePassNo);

    public class GoodsReceiptNoteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "GRN Number")]
        public string? GRNNumber { get; set; }

        [Required, Display(Name = "GRN Date")]
        public DateTime GRNDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Display(Name = "Purchase Order")]
        public int? PurchaseOrderId { get; set; }

        [Display(Name = "Gate Pass")]
        public int? InwardGatePassId { get; set; }

        [Required, Display(Name = "Receiving Location")]
        public int LocationId { get; set; }

        public string? Remarks { get; set; }

        public List<GoodsReceiptNoteItemViewModel> Items { get; set; } = new() { new() };

        public List<VendorOption> Vendors { get; set; } = new();
        public List<PurchaseOrderOption> PurchaseOrders { get; set; } = new();
        public List<GatePassOption> GatePasses { get; set; } = new();
        public List<LocationOption> Locations { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class GoodsReceiptNoteItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DeclaredQuantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AcceptedQuantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RejectedQuantity { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal UnitCost { get; set; }

        public string? Remarks { get; set; }
    }

    public class GoodsReceiptNoteListItemViewModel
    {
        public int Id { get; set; }
        public string GRNNumber { get; set; } = string.Empty;
        public DateTime GRNDate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
