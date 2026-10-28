using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Purchase;

namespace ERP.Identity.Models.ViewModels.Purchase
{
    public class IssuedChequeViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Reference No.")]
        public string? ChequeReferenceNo { get; set; }

        [Required, Display(Name = "Cheque Number")]
        public string ChequeNumber { get; set; } = string.Empty;

        [Required, Display(Name = "Bank Name")]
        public string BankName { get; set; } = string.Empty;

        [Required, Display(Name = "Cheque Date")]
        public DateTime ChequeDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        public string? Remarks { get; set; }

        public List<VendorOption> Vendors { get; set; } = new();
    }

    public class IssuedChequeListItemViewModel
    {
        public int Id { get; set; }
        public string ChequeReferenceNo { get; set; } = string.Empty;
        public string ChequeNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public DateTime ChequeDate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ClearanceStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class VendorAdvanceViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Advance Number")]
        public string? AdvanceNumber { get; set; }

        [Required, Display(Name = "Advance Date")]
        public DateTime AdvanceDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "Issued Cheque")]
        public int? IssuedChequeId { get; set; }

        public string? Remarks { get; set; }

        public List<VendorOption> Vendors { get; set; } = new();
        public List<ChequeOption> Cheques { get; set; } = new();
    }

    public class VendorAdvanceListItemViewModel
    {
        public int Id { get; set; }
        public string AdvanceNumber { get; set; } = string.Empty;
        public DateTime AdvanceDate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public decimal AvailableAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
