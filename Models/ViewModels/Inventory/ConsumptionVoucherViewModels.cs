using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.ViewModels.Inventory
{
    public class ConsumptionVoucherViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Voucher No.")]
        public string? VoucherNo { get; set; }

        [Required, Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Location")]
        public int LocationId { get; set; }

        public string? Department { get; set; }

        [Display(Name = "Issued To")]
        public string? IssuedTo { get; set; }

        public string? Purpose { get; set; }

        public string? Remarks { get; set; }

        public List<ConsumptionVoucherItemViewModel> Items { get; set; } = new() { new() };

        public List<LocationOption> Locations { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class ConsumptionVoucherItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        public string? Remarks { get; set; }
    }

    public class ConsumptionVoucherListItemViewModel
    {
        public int Id { get; set; }
        public string VoucherNo { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? IssuedTo { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
