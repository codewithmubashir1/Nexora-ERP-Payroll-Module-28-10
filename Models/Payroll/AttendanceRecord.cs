using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Accounts;

namespace ERP.Identity.Models.Payroll
{
    public enum AttendanceStatus
    {
        Present = 0,
        Absent = 1,
        Leave = 2,
        Holiday = 3,
        WeekOff = 4,
        HalfDay = 5
    }

    public class AttendanceRecord
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime Date { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal WorkedHours { get; set; }

        public bool IsLate { get; set; }
        public int LateByMinutes { get; set; }

        public bool IsEarlyDeparture { get; set; }
        public int EarlyByMinutes { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
