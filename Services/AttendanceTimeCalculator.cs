using ERP.Identity.Models.Payroll;

namespace ERP.Identity.Services
{
    public static class AttendanceTimeCalculator
    {
        public static decimal WorkedHours(TimeSpan? checkIn, TimeSpan? checkOut, int breakMinutes)
        {
            if (!checkIn.HasValue || !checkOut.HasValue) return 0m;

            var elapsed = checkOut.Value >= checkIn.Value
                ? checkOut.Value - checkIn.Value
                : (TimeSpan.FromHours(24) - checkIn.Value) + checkOut.Value;
            var netHours = (decimal)elapsed.TotalHours - Math.Max(0, breakMinutes) / 60m;
            return Math.Round(Math.Max(0m, netHours), 2, MidpointRounding.AwayFromZero);
        }

        public static decimal WorkedHours(AttendanceRecord record, int breakMinutes) =>
            record.WorkedHours > 0
                ? record.WorkedHours
                : WorkedHours(record.CheckInTime, record.CheckOutTime, breakMinutes);

        public static decimal OvertimeHours(AttendanceRecord record, decimal shiftHours, int breakMinutes)
        {
            var hoursWorked = WorkedHours(record, breakMinutes);
            if (record.Status is AttendanceStatus.Holiday or AttendanceStatus.WeekOff) return hoursWorked;
            return Math.Max(0m, hoursWorked - shiftHours);
        }
    }
}