using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Payroll
{
    public enum AttendancePolicyType
    {
        LateComing = 0,
        LateSitting = 1,
        EarlyGoing = 2,
        HolidayWorking = 3
    }

    public enum PenaltyType
    {
        None = 0,
        Warning = 1,
        HalfDayDeduction = 2,
        FullDayDeduction = 3
    }

    /// <summary>One configurable rule per policy type (4 rows total — LateComing, LateSitting, EarlyGoing, HolidayWorking). Seeded on first run so Settings always has all four to edit.</summary>
    public class AttendancePolicy
    {
        public int Id { get; set; }

        public AttendancePolicyType PolicyType { get; set; }

        /// <summary>Minutes of tolerance before this rule kicks in (grace period for late coming/early going, or minimum extra minutes to count as "sitting late").</summary>
        public int GracePeriodMinutes { get; set; } = 10;

        /// <summary>How many occurrences per month are tolerated before the penalty applies.</summary>
        public int MaxOccurrencesPerMonth { get; set; } = 3;

        public PenaltyType PenaltyOnExceeding { get; set; } = PenaltyType.None;

        /// <summary>For Holiday Working specifically — how working on a holiday is compensated.</summary>
        [MaxLength(50)]
        public string? CompensationType { get; set; } // "CompOff" | "DoublePay" | "OvertimeRate" | null

        [Column(TypeName = "decimal(5,2)")]
        public decimal? OvertimeRateMultiplier { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Singleton company-wide payroll settings, self-seeding on first access (same pattern as CompanySettings).</summary>
    public class OtherPayrollSettings
    {
        public int Id { get; set; }

        public int WorkingDaysPerWeek { get; set; } = 6;

        public int FiscalYearStartMonth { get; set; } = 7; // July, common in Pakistan

        [MaxLength(10)]
        public string DefaultCurrency { get; set; } = "PKR";

        [Column(TypeName = "decimal(5,2)")]
        public decimal OvertimeRateMultiplier { get; set; } = 1.5m;

        public bool RoundOffNetPay { get; set; } = true;

        public int PayrollCutoffDay { get; set; } = 25;

        /// <summary>Flat amount deducted from every employee's salary each month (e.g. a welfare/ledger contribution), posted to their Employee ledger on Pay Run approval.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyFixedDeduction { get; set; } = 300m;

        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    }

    public enum AttendanceDeviceType
    {
        Biometric = 0,
        RFIDCard = 1,
        Manual = 2
    }

    public class AttendanceDevice
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Location { get; set; }

        public AttendanceDeviceType DeviceType { get; set; } = AttendanceDeviceType.Biometric;

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        [MaxLength(50)]
        public string? SerialNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastSyncOn { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Shared integration settings for inbound biometric attendance APIs, including ZKTeco ADMS push.</summary>
    public class MachineIntegrationSetting
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string SyncMode { get; set; } = "Manual"; // "Manual" | "Scheduled"

        public int SyncIntervalMinutes { get; set; } = 60;

        [MaxLength(300)]
        public string? ApiEndpoint { get; set; }

        [MaxLength(150)]
        public string? ApiKey { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    }
}
