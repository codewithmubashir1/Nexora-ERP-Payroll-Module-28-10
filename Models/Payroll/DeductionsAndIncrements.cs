using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Accounts;

namespace ERP.Identity.Models.Payroll
{
    /// <summary>One income-tax slab, e.g. "Rs 0 - 50,000 @ 0%" or "Rs 50,001+ @ 2.5% above 50,000". Multiple rows make up the full progressive slab table.</summary>
    public class TaxSlab
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinIncome { get; set; }

        /// <summary>Null means "and above" — the top open-ended slab.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxIncome { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RatePercent { get; set; }

        /// <summary>Flat amount added on top of the percentage — standard for progressive slabs (e.g. "Rs 1,250 + 5% of amount over 100,000").</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal FixedAmount { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Simple percentage-of-salary statutory deductions other than income tax — EOBI, Social Security, Provident Fund, etc.</summary>
    public class StatutoryDeduction
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal RatePercent { get; set; }

        /// <summary>Optional cap — some statutory deductions (like EOBI) apply only up to a wage ceiling.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxApplicableSalary { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public class SalaryIncrement
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OldBasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewBasicSalary { get; set; }

        [MaxLength(300)]
        public string? Reason { get; set; }

        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
