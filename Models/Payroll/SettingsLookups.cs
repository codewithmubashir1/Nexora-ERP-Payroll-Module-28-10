using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Payroll
{
    public class LeaveType
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int DaysPerYear { get; set; }

        public bool IsPaid { get; set; } = true;

        /// <summary>Whether unused days of this leave type can be encashed via a Leave Encashment Request.</summary>
        public bool IsEncashable { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public class Holiday
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Optional holidays don't count against attendance if the employee chooses to work.</summary>
        public bool IsOptional { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public class ReimbursementType
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxLimit { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
