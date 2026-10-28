using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ERP.Identity.Data;
using ERP.Identity.Models.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/zkteco/iclock")]
    public sealed class ZktecoPushController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ZktecoPushController(ApplicationDbContext db) => _db = db;

        /// <summary>Returns device options for a registered ZKTeco push device.</summary>
        [HttpGet("cdata")]
        [Produces("text/plain")]
        public async Task<IActionResult> GetDeviceOptions()
        {
            var device = await AuthenticateDeviceAsync();
            if (device == null) return PlainText("Unauthorized", StatusCodes.Status401Unauthorized);

            var response = $"GET OPTION FROM: {device.SerialNumber}\n" +
                "Stamp=0\nOpStamp=0\nErrorDelay=30\nDelay=10\n" +
                "TransTimes=00:00;14:05\nTransInterval=1\nTransFlag=1111000000\n" +
                "Realtime=1\nEncrypt=0\n";
            return PlainText(response);
        }

        /// <summary>Responds to the ZKTeco ADMS command poll when no commands are queued.</summary>
        [HttpGet("getrequest")]
        [Produces("text/plain")]
        public async Task<IActionResult> GetPendingCommands()
        {
            var device = await AuthenticateDeviceAsync();
            return device == null
                ? PlainText("Unauthorized", StatusCodes.Status401Unauthorized)
                : PlainText("OK");
        }

        /// <summary>Receives tab-delimited ATTLOG rows from a configured ZKTeco push device.</summary>
        [HttpPost("cdata")]
        [Consumes("text/plain", "application/octet-stream", "application/x-www-form-urlencoded")]
        [RequestSizeLimit(1_048_576)]
        [Produces("text/plain")]
        public async Task<IActionResult> ReceiveAttendance()
        {
            var device = await AuthenticateDeviceAsync();
            if (device == null) return PlainText("Unauthorized", StatusCodes.Status401Unauthorized);

            var table = Request.Query["table"].ToString();
            if (!string.IsNullOrWhiteSpace(table) && !string.Equals(table, "ATTLOG", StringComparison.OrdinalIgnoreCase))
                return PlainText("OK");

            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var payload = await reader.ReadToEndAsync(HttpContext.RequestAborted);
            var punches = ParsePunches(payload);
            if (punches.Count == 0)
            {
                device.LastSyncOn = DateTime.UtcNow;
                await _db.SaveChangesAsync(HttpContext.RequestAborted);
                return PlainText("OK: 0");
            }

            var pins = punches.Select(punch => punch.Pin).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var numericPins = pins
                .Select(pin => int.TryParse(pin, NumberStyles.Integer, CultureInfo.InvariantCulture, out var employeeId) ? employeeId : (int?)null)
                .Where(employeeId => employeeId.HasValue)
                .Select(employeeId => employeeId!.Value)
                .ToList();

            var employees = await _db.Employees
                .Include(employee => employee.ShiftTiming)
                .Where(employee => employee.IsActive &&
                    (pins.Contains(employee.EmployeeCode) || numericPins.Contains(employee.Id)))
                .ToListAsync(HttpContext.RequestAborted);

            var employeeByPin = new Dictionary<string, ERP.Identity.Models.Accounts.Employee>(StringComparer.OrdinalIgnoreCase);
            foreach (var employee in employees)
            {
                employeeByPin[employee.EmployeeCode] = employee;
                employeeByPin[employee.Id.ToString(CultureInfo.InvariantCulture)] = employee;
            }

            var matchedPunches = punches
                .Where(punch => employeeByPin.ContainsKey(punch.Pin))
                .Select(punch => new MatchedPunch(employeeByPin[punch.Pin], punch.Timestamp))
                .GroupBy(punch => new { punch.Employee.Id, Date = punch.Timestamp.Date });

            var policies = await _db.AttendancePolicies.ToListAsync(HttpContext.RequestAborted);
            var lateGrace = policies.FirstOrDefault(policy => policy.PolicyType == AttendancePolicyType.LateComing)?.GracePeriodMinutes ?? 0;
            var earlyGrace = policies.FirstOrDefault(policy => policy.PolicyType == AttendancePolicyType.EarlyGoing)?.GracePeriodMinutes ?? 0;
            var importedDays = 0;

            foreach (var dayPunches in matchedPunches)
            {
                var employee = dayPunches.First().Employee;
                var day = dayPunches.Key.Date;
                var record = await _db.AttendanceRecords
                    .FirstOrDefaultAsync(attendance => attendance.EmployeeId == employee.Id && attendance.Date == day,
                        HttpContext.RequestAborted);

                if (record == null)
                {
                    record = new AttendanceRecord
                    {
                        EmployeeId = employee.Id,
                        Date = day,
                        CreatedOn = DateTime.UtcNow
                    };
                    _db.AttendanceRecords.Add(record);
                }

                var eventTimes = dayPunches.Select(punch => punch.Timestamp.TimeOfDay).ToList();
                if (record.CheckInTime.HasValue) eventTimes.Add(record.CheckInTime.Value);
                if (record.CheckOutTime.HasValue) eventTimes.Add(record.CheckOutTime.Value);
                var orderedTimes = eventTimes.Distinct().OrderBy(time => time).ToList();

                if (orderedTimes.Count > 0)
                {
                    record.Status = AttendanceStatus.Present;
                    record.CheckInTime = orderedTimes[0];
                    record.CheckOutTime = orderedTimes.Count > 1 ? orderedTimes[^1] : null;
                    record.IsLate = false;
                    record.LateByMinutes = 0;
                    record.IsEarlyDeparture = false;
                    record.EarlyByMinutes = 0;

                    var shift = employee.ShiftTiming;
                    if (shift != null)
                    {
                        var lateBy = (int)(record.CheckInTime.Value - shift.StartTime).TotalMinutes - shift.GracePeriodMinutes - lateGrace;
                        if (lateBy > 0)
                        {
                            record.IsLate = true;
                            record.LateByMinutes = lateBy;
                        }

                        if (record.CheckOutTime.HasValue)
                        {
                            var earlyBy = (int)(shift.EndTime - record.CheckOutTime.Value).TotalMinutes - earlyGrace;
                            if (earlyBy > 0)
                            {
                                record.IsEarlyDeparture = true;
                                record.EarlyByMinutes = earlyBy;
                            }
                        }
                    }
                }

                importedDays++;
            }

            device.LastSyncOn = DateTime.UtcNow;
            await _db.SaveChangesAsync(HttpContext.RequestAborted);
            return PlainText($"OK: {importedDays}");
        }

        private async Task<AttendanceDevice?> AuthenticateDeviceAsync()
        {
            var serialNumber = Request.Query["SN"].ToString().Trim();
            if (serialNumber.Length == 0) return null;

            var device = await _db.AttendanceDevices.FirstOrDefaultAsync(candidate =>
                candidate.SerialNumber != null && candidate.SerialNumber.ToLower() == serialNumber.ToLower() &&
                candidate.IsActive && candidate.DeviceType == AttendanceDeviceType.Biometric,
                HttpContext.RequestAborted);
            if (device == null) return null;

            var settings = await _db.MachineIntegrationSettings.OrderBy(setting => setting.Id).FirstOrDefaultAsync(HttpContext.RequestAborted);
            if (string.IsNullOrWhiteSpace(settings?.ApiKey)) return null;

            var suppliedKey = Request.Query["KEY"].FirstOrDefault()
                ?? Request.Query["key"].FirstOrDefault()
                ?? Request.Headers["X-Api-Key"].FirstOrDefault();
            var authorization = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(suppliedKey) && authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
                suppliedKey = authorization[7..].Trim();

            if (string.IsNullOrWhiteSpace(suppliedKey)) return null;
            var expectedBytes = Encoding.UTF8.GetBytes(settings.ApiKey);
            var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
            return expectedBytes.Length == suppliedBytes.Length && CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes)
                ? device
                : null;
        }

        private static List<DevicePunch> ParsePunches(string payload)
        {
            var punches = new List<DevicePunch>();
            foreach (var line in payload.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var fields = line.Split('\t');
                if (fields.Length < 2 || string.IsNullOrWhiteSpace(fields[0])) continue;

                if (DateTime.TryParse(fields[1].Trim(), CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces, out var timestamp))
                    punches.Add(new DevicePunch(fields[0].Trim(), DateTime.SpecifyKind(timestamp, DateTimeKind.Unspecified)));
            }

            return punches;
        }

        private ContentResult PlainText(string content, int statusCode = StatusCodes.Status200OK) => new()
        {
            Content = content,
            ContentType = "text/plain; charset=utf-8",
            StatusCode = statusCode
        };

        private sealed record DevicePunch(string Pin, DateTime Timestamp);
        private sealed record MatchedPunch(ERP.Identity.Models.Accounts.Employee Employee, DateTime Timestamp);
    }
}
