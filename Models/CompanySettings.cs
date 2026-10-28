using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models
{
    /// <summary>
    /// Singleton settings row (always Id = 1) holding the company's own letterhead
    /// info — used to render "Our Information" and Bank Details on every printed
    /// document across every module.
    /// </summary>
    public class CompanySettings
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = "Your Company Name";

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        /// <summary>Format: 0000000-0</summary>
        [MaxLength(15)]
        public string? NTNNumber { get; set; }

        /// <summary>Format: 00-00-0000-000-00</summary>
        [MaxLength(20)]
        public string? SalesTaxRegistrationNumber { get; set; }

        public string? LogoPath { get; set; }

        [MaxLength(150)]
        public string? BankName { get; set; }

        [MaxLength(150)]
        public string? BankAccountTitle { get; set; }

        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    }
}
