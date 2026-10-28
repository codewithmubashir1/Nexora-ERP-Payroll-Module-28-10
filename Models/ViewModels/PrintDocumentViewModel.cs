namespace ERP.Identity.Models.ViewModels
{
    public class PrintDocumentViewModel
    {
        public string DocumentTypeTitle { get; set; } = string.Empty; // e.g. "PURCHASE ORDER"
        public string DocumentNumberLabel { get; set; } = string.Empty; // e.g. "PO #"
        public string DocumentNumber { get; set; } = string.Empty;
        public string? Status { get; set; }

        /// <summary>Secondary reference rows in the top-right info box (date, linked docs, etc.).</summary>
        public List<(string Label, string Value)> ExtraRefs { get; set; } = new();

        public string PartyLabel { get; set; } = "VENDOR"; // or "CUSTOMER"
        public string? PartyCode { get; set; }
        public string? PartyVendorCode { get; set; }
        public string? PartyName { get; set; }
        public string? PartyAddress { get; set; }
        public string? PartyEmail { get; set; }
        public string? PartyPhone { get; set; }
        public string? PartyNTN { get; set; }
        public string? PartySTRN { get; set; }

        public List<string> Columns { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();

        /// <summary>Right-side totals box: label, value, bold?</summary>
        public List<(string Label, string Value, bool Bold)> Totals { get; set; } = new();

        public string? AmountInWords { get; set; }
        public string? TaxAmountInWords { get; set; }
        public string? AdditionalNotes { get; set; }

        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }

        public bool ShowBankDetails { get; set; } = true;

        public CompanySettings Company { get; set; } = new();
    }
}
