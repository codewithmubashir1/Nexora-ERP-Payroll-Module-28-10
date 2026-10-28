using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Accounts;

namespace ERP.Identity.Models.ViewModels.Accounts
{
    public record ItemAdjustmentNoteOption(int Id, string AdjustmentNo);

    public class InventoryVoucherViewModel
    {
        public int Id { get; set; }
        public string? VoucherNumber { get; set; }

        [Required, Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Voucher Type")]
        public InventoryVoucherType VoucherType { get; set; } = InventoryVoucherType.Decrease;

        [Required, Display(Name = "Inventory Account")]
        public int InventoryAccountId { get; set; }

        [Required, Display(Name = "Contra Account")]
        public int ContraAccountId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Display(Name = "Related Adjustment Note")]
        public int? ItemAdjustmentNoteId { get; set; }

        public string? Narration { get; set; }

        public List<AccountOption> InventoryAccounts { get; set; } = new();
        public List<AccountOption> ContraAccounts { get; set; } = new();
        public List<ItemAdjustmentNoteOption> AdjustmentNotes { get; set; } = new();
    }

    public class InventoryVoucherListItemViewModel
    {
        public int Id { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public string ContraAccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
