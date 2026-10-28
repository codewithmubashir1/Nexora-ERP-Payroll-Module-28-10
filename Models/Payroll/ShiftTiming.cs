using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Payroll
{
    public class ShiftTiming
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        /// <summary>Unpaid break minutes deducted from elapsed attendance time.</summary>
        public int BreakMinutes { get; set; }

        /// <summary>Minutes an employee can check in after StartTime before being marked late.</summary>
        public int GracePeriodMinutes { get; set; } = 10;

        /// <summary>Shift length in hours, handling shifts that cross midnight. Not mapped — derived from Start/End.</summary>
        [NotMapped]
        public decimal ShiftHours
        {
            get
            {
                var span = EndTime >= StartTime ? EndTime - StartTime : (TimeSpan.FromHours(24) - StartTime) + EndTime;
                return Math.Max(0m, (decimal)span.TotalHours - BreakMinutes / 60m);
            }
        }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
