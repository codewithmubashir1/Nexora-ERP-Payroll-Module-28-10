using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.ViewModels.Inventory
{
    public class ItemAdjustmentNoteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Adjustment No.")]
        public string? AdjustmentNo { get; set; }

        [Required, Display(Name = "Adjustment Date")]
        public DateTime AdjustmentDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Location")]
        public int LocationId { get; set; }

        public string? Reason { get; set; }

        public string? Remarks { get; set; }

        public List<ItemAdjustmentNoteItemViewModel> Items { get; set; } = new() { new() };

        public List<LocationOption> Locations { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class ItemAdjustmentNoteItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        public AdjustmentType AdjustmentType { get; set; } = AdjustmentType.Increase;

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        public string? Remarks { get; set; }
    }

    public class ItemAdjustmentNoteListItemViewModel
    {
        public int Id { get; set; }
        public string AdjustmentNo { get; set; } = string.Empty;
        public DateTime AdjustmentDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
