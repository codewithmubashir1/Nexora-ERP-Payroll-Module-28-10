using System.Globalization;
using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>
    /// Daily attendance capture and administrator tools for configured attendance devices.
    /// Late-coming and early-going are derived from employee shifts and policies.
    /// </summary>
    [Authorize]
    [Route("Payroll/Attendance")]
    public class AttendanceController : Controller
    {
        private const string EditorRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;

        public AttendanceController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public async Task<IActionResult> Index(DateTime? date, DateTime? endDate, int? departmentId)
        {
            var start = (date ?? DateTime.Today).Date;
            var end = (endDate ?? start).Date;
            if (end < start) end = start;
            if ((end - start).TotalDays > 30) end = start.AddDays(30);
            var vm = new AttendanceEntryViewModel
            {
                Date = start,
                EndDate = end,
                DepartmentId = departmentId
            };

            await LoadRowsAsync(vm);
            return View(vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EditorRoles)]
        public async Task<IActionResult> Save(AttendanceEntryViewModel model)
        {
            if (model.Rows == null || model.Rows.Count == 0)
            {
                TempData["ErrorMessage"] = "Nothing to save — no employees were listed.";
                return RedirectToAction(nameof(Index), new { date = model.Date.ToString("yyyy-MM-dd"), endDate = model.EndDate.ToString("yyyy-MM-dd"), departmentId = model.DepartmentId });
            }

            var date = model.Date.Date;
            var endDate = model.EndDate.Date < date ? date : model.EndDate.Date;
            if ((endDate - date).TotalDays > 30)
            {
                TempData["ErrorMessage"] = "Attendance entry supports a maximum 31-day range.";
                return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd"), departmentId = model.DepartmentId });
            }
            var employeeIds = model.Rows.Select(r => r.EmployeeId).ToList();

            var employees = await _db.Employees
                .Include(e => e.ShiftTiming)
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();

            var existing = await _db.AttendanceRecords
                .Where(a => employeeIds.Contains(a.EmployeeId) && a.Date >= date && a.Date <= endDate)
                .ToListAsync();

            var policies = await _db.AttendancePolicies.ToListAsync();
            var lateGrace = policies.FirstOrDefault(p => p.PolicyType == AttendancePolicyType.LateComing)?.GracePeriodMinutes ?? 0;
            var earlyGrace = policies.FirstOrDefault(p => p.PolicyType == AttendancePolicyType.EarlyGoing)?.GracePeriodMinutes ?? 0;

            var saved = 0;

            foreach (var row in model.Rows)
            {
                var employee = employees.FirstOrDefault(e => e.Id == row.EmployeeId);
                if (employee == null) continue;
                var rowDate = row.Date.Date;
                if (rowDate < date || rowDate > endDate) continue;

                var record = existing.FirstOrDefault(a => a.EmployeeId == row.EmployeeId && a.Date == rowDate);
                if (record == null)
                {
                    record = new AttendanceRecord
                    {
                        EmployeeId = row.EmployeeId,
                        Date = rowDate,
                        CreatedOn = DateTime.UtcNow
                    };
                    _db.AttendanceRecords.Add(record);
                }

                record.Status = row.Status;
                record.Remarks = row.Remarks;

                var hasCheckIn = TryParseTime(row.CheckInTime, out var checkIn);
                var hasCheckOut = TryParseTime(row.CheckOutTime, out var checkOut);

                // Times only make sense on a day that was actually worked.
                var worked = row.Status is AttendanceStatus.Present or AttendanceStatus.HalfDay
                             or AttendanceStatus.Holiday or AttendanceStatus.WeekOff;

                record.CheckInTime = worked && hasCheckIn ? checkIn : null;
                record.CheckOutTime = worked && hasCheckOut ? checkOut : null;
                record.WorkedHours = worked
                    ? AttendanceTimeCalculator.WorkedHours(record.CheckInTime, record.CheckOutTime, employee.ShiftTiming?.BreakMinutes ?? 0)
                    : 0m;

                record.IsLate = false;
                record.LateByMinutes = 0;
                record.IsEarlyDeparture = false;
                record.EarlyByMinutes = 0;

                var shift = employee.ShiftTiming;
                if (shift != null && worked && row.Status != AttendanceStatus.Holiday && row.Status != AttendanceStatus.WeekOff)
                {
                    // Grace comes from the shift itself, widened by the Late Coming policy.
                    var totalLateGrace = shift.GracePeriodMinutes + lateGrace;

                    if (record.CheckInTime.HasValue)
                    {
                        var lateBy = (int)(record.CheckInTime.Value - shift.StartTime).TotalMinutes - totalLateGrace;
                        if (lateBy > 0)
                        {
                            record.IsLate = true;
                            record.LateByMinutes = lateBy;
                        }
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

                saved++;
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Attendance saved for {saved} row(s) from {date:dd MMM yyyy} through {endDate:dd MMM yyyy}.";
            return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd"), endDate = endDate.ToString("yyyy-MM-dd"), departmentId = model.DepartmentId });
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export(DateTime? date, DateTime? endDate, int? departmentId)
        {
            var day = (date ?? DateTime.Today).Date;
            var lastDay = (endDate ?? day).Date < day ? day : (endDate ?? day).Date;
            if ((lastDay - day).TotalDays > 30) return BadRequest("Attendance export supports a maximum 31-day range.");
            var query = _db.AttendanceRecords
                .Include(a => a.Employee).ThenInclude(e => e!.DepartmentRef)
                .Include(a => a.Employee).ThenInclude(e => e!.ShiftTiming)
                .Where(a => a.Date >= day && a.Date <= lastDay);
            if (departmentId.HasValue) query = query.Where(a => a.Employee!.DepartmentId == departmentId.Value);

            var records = await query.OrderBy(a => a.Employee!.EmployeeCode).ToListAsync();
            var headers = new[] { "EmployeeCode", "Employee", "Date", "Status", "CheckInTime", "CheckOutTime", "WorkedHours", "LateMinutes", "EarlyMinutes", "Remarks" };
            var rows = records.Select(record => new string?[]
            {
                record.Employee?.EmployeeCode, record.Employee?.Name, record.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                record.Status.ToString(), record.CheckInTime?.ToString(@"hh\:mm"), record.CheckOutTime?.ToString(@"hh\:mm"),
                AttendanceTimeCalculator.WorkedHours(record, record.Employee?.ShiftTiming?.BreakMinutes ?? 0).ToString("0.00", CultureInfo.InvariantCulture),
                record.LateByMinutes.ToString(CultureInfo.InvariantCulture), record.EarlyByMinutes.ToString(CultureInfo.InvariantCulture), record.Remarks
            });
            return File(CsvUtility.WriteWithHeader(headers, rows), "text/csv", $"attendance_{day:yyyyMMdd}_{lastDay:yyyyMMdd}.csv");
        }

        [HttpGet("ImportTemplate")]
        public IActionResult ImportTemplate()
        {
            var headers = new[] { "EmployeeCode", "Date", "Status", "CheckInTime", "CheckOutTime", "Remarks" };
            var example = new[] { "EMP-000001", "2026-09-29", "Present", "08:00", "17:00", "" };
            return File(CsvUtility.WriteWithHeader(headers, new[] { example }), "text/csv", "attendance_template.csv");
        }

        [HttpPost("Import")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EditorRoles)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Choose a non-empty CSV file." });

            var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
            var employees = await _db.Employees.Include(e => e.ShiftTiming).Where(e => e.IsActive).ToListAsync();
            var byCode = employees.ToDictionary(e => e.EmployeeCode, StringComparer.OrdinalIgnoreCase);
            var parsed = new List<(int RowNumber, Employee Employee, DateTime Date, AttendanceStatus Status, TimeSpan? CheckIn, TimeSpan? CheckOut, string? Remarks)>();
            var errors = new List<string>();

            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                var rowNumber = index + 2;
                var code = row.Get("EmployeeCode");
                if (string.IsNullOrWhiteSpace(code) || !byCode.TryGetValue(code, out var employee))
                {
                    errors.Add($"Row {rowNumber}: EmployeeCode is missing or unknown.");
                    continue;
                }

                if (!DateTime.TryParseExact(row.Get("Date"), new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var attendanceDate))
                {
                    errors.Add($"Row {rowNumber}: Date must use yyyy-MM-dd or dd/MM/yyyy format.");
                    continue;
                }

                var statusText = row.Get("Status");
                if (!Enum.TryParse(statusText, true, out AttendanceStatus status) || !Enum.IsDefined(typeof(AttendanceStatus), status))
                {
                    errors.Add($"Row {rowNumber}: Status must be Present, Absent, Leave, Holiday, WeekOff, or HalfDay.");
                    continue;
                }

                var checkInText = row.Get("CheckInTime");
                var checkOutText = row.Get("CheckOutTime");
                if ((!string.IsNullOrWhiteSpace(checkInText) && !TryParseTime(checkInText, out _)) ||
                    (!string.IsNullOrWhiteSpace(checkOutText) && !TryParseTime(checkOutText, out _)))
                {
                    errors.Add($"Row {rowNumber}: Check-in and check-out must use HH:mm format.");
                    continue;
                }
                TryParseTime(checkInText, out var checkIn);
                TryParseTime(checkOutText, out var checkOut);
                parsed.Add((rowNumber, employee, attendanceDate.Date, status,
                    string.IsNullOrWhiteSpace(checkInText) ? (TimeSpan?)null : checkIn,
                    string.IsNullOrWhiteSpace(checkOutText) ? (TimeSpan?)null : checkOut,
                    row.Get("Remarks")));
            }

            var employeeIds = parsed.Select(item => item.Employee.Id).Distinct().ToList();
            var dates = parsed.Select(item => item.Date).ToList();
            var records = dates.Count == 0
                ? new List<AttendanceRecord>()
                : await _db.AttendanceRecords.Where(a => employeeIds.Contains(a.EmployeeId) && a.Date >= dates.Min() && a.Date <= dates.Max()).ToListAsync();
            var policies = await _db.AttendancePolicies.ToListAsync();
            var lateGrace = policies.FirstOrDefault(p => p.PolicyType == AttendancePolicyType.LateComing)?.GracePeriodMinutes ?? 0;
            var earlyGrace = policies.FirstOrDefault(p => p.PolicyType == AttendancePolicyType.EarlyGoing)?.GracePeriodMinutes ?? 0;
            var saved = 0;

            foreach (var item in parsed)
            {
                var record = records.FirstOrDefault(a => a.EmployeeId == item.Employee.Id && a.Date == item.Date);
                if (record == null)
                {
                    record = new AttendanceRecord { EmployeeId = item.Employee.Id, Date = item.Date, CreatedOn = DateTime.UtcNow };
                    _db.AttendanceRecords.Add(record);
                    records.Add(record);
                }

                var worked = item.Status is AttendanceStatus.Present or AttendanceStatus.HalfDay or AttendanceStatus.Holiday or AttendanceStatus.WeekOff;
                record.Status = item.Status;
                record.CheckInTime = worked ? item.CheckIn : null;
                record.CheckOutTime = worked ? item.CheckOut : null;
                record.WorkedHours = worked ? AttendanceTimeCalculator.WorkedHours(record.CheckInTime, record.CheckOutTime, item.Employee.ShiftTiming?.BreakMinutes ?? 0) : 0m;
                record.Remarks = item.Remarks;
                record.IsLate = false;
                record.LateByMinutes = 0;
                record.IsEarlyDeparture = false;
                record.EarlyByMinutes = 0;

                var shift = item.Employee.ShiftTiming;
                if (shift != null && worked && item.Status is not (AttendanceStatus.Holiday or AttendanceStatus.WeekOff))
                {
                    if (record.CheckInTime.HasValue)
                    {
                        var lateBy = (int)(record.CheckInTime.Value - shift.StartTime).TotalMinutes - shift.GracePeriodMinutes - lateGrace;
                        if (lateBy > 0) { record.IsLate = true; record.LateByMinutes = lateBy; }
                    }
                    if (record.CheckOutTime.HasValue)
                    {
                        var earlyBy = (int)(shift.EndTime - record.CheckOutTime.Value).TotalMinutes - earlyGrace;
                        if (earlyBy > 0) { record.IsEarlyDeparture = true; record.EarlyByMinutes = earlyBy; }
                    }
                }
                saved++;
            }

            await _db.SaveChangesAsync();
            var summary = $"Imported {saved} attendance row(s); skipped {errors.Count}.";
            return Json(new { success = errors.Count == 0, errorCount = errors.Count, summary, errors });
        }

        /// <summary>
        /// Marks every listed employee with one status in a single click — the usual
        /// way a supervisor handles a public holiday or a weekly off.
        /// </summary>
        [HttpPost("BulkMark")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EditorRoles)]
        public async Task<IActionResult> BulkMark(DateTime date, int? departmentId, AttendanceStatus status)
        {
            var query = _db.Employees.Where(e => e.IsActive);
            if (departmentId.HasValue) query = query.Where(e => e.DepartmentId == departmentId.Value);

            var employeeIds = await query.Select(e => e.Id).ToListAsync();
            var day = date.Date;

            var existing = await _db.AttendanceRecords
                .Where(a => employeeIds.Contains(a.EmployeeId) && a.Date == day)
                .ToListAsync();

            foreach (var employeeId in employeeIds)
            {
                var record = existing.FirstOrDefault(a => a.EmployeeId == employeeId);
                if (record == null)
                {
                    record = new AttendanceRecord { EmployeeId = employeeId, Date = day, CreatedOn = DateTime.UtcNow };
                    _db.AttendanceRecords.Add(record);
                }

                record.Status = status;
                record.CheckInTime = null;
                record.CheckOutTime = null;
                record.IsLate = false;
                record.LateByMinutes = 0;
                record.IsEarlyDeparture = false;
                record.EarlyByMinutes = 0;
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"{employeeIds.Count} employee(s) marked as {status} on {day:dd MMM yyyy}." });
        }

        /// <summary>
        /// Pulls punches from a configured attendance device. Real biometric hardware
        /// needs a vendor-specific driver, so this reads whatever the device's HTTP
        /// endpoint exposes and records the sync timestamp; wire the actual protocol in
        /// where marked.
        /// </summary>
        [HttpPost("SyncDevice")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EditorRoles)]
        public async Task<IActionResult> SyncDevice(int deviceId)
        {
            var device = await _db.AttendanceDevices.FindAsync(deviceId);
            if (device == null) return Json(new { success = false, message = "Device not found." });
            if (!device.IsActive) return Json(new { success = false, message = "This device is inactive." });
            if (device.DeviceType != AttendanceDeviceType.Biometric || string.IsNullOrWhiteSpace(device.SerialNumber))
                return Json(new { success = false, message = "Configure this as a biometric device and set its serial number first." });

            var settings = await _db.MachineIntegrationSettings.OrderBy(setting => setting.Id).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(settings?.ApiKey))
                return Json(new { success = false, message = "Set and save the shared API key in Payroll Settings before connecting a ZKTeco device." });

            var endpoint = $"{Request.Scheme}://{Request.Host}/api/zkteco/iclock/cdata";

            return Json(new
            {
                success = true,
                message = $"ZKTeco push is ready for {device.Name}. Configure its server URL as {endpoint}; punches will update Last Sync when received."
            });
        }

        private async Task LoadRowsAsync(AttendanceEntryViewModel vm)
        {
            vm.Departments = await _db.Departments.Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentOption(d.Id, d.Name, d.Code))
                .ToListAsync();

            vm.HolidayName = await _db.Holidays
                .Where(h => h.Date == vm.Date.Date)
                .Select(h => h.Name)
                .FirstOrDefaultAsync();

            var query = _db.Employees
                .Include(e => e.DepartmentRef)
                .Include(e => e.ShiftTiming)
                .Where(e => e.IsActive);

            if (vm.DepartmentId.HasValue)
                query = query.Where(e => e.DepartmentId == vm.DepartmentId.Value);

            var employees = await query.OrderBy(e => e.Name).ToListAsync();
            var employeeIds = employees.Select(e => e.Id).ToList();

            var existing = await _db.AttendanceRecords
                .Where(a => employeeIds.Contains(a.EmployeeId) && a.Date >= vm.Date.Date && a.Date <= vm.EndDate.Date)
                .ToListAsync();

            vm.Rows = employees.SelectMany(e => Enumerable.Range(0, (vm.EndDate.Date - vm.Date.Date).Days + 1).Select(dayOffset =>
            {
                var rowDate = vm.Date.Date.AddDays(dayOffset);
                var record = existing.FirstOrDefault(a => a.EmployeeId == e.Id && a.Date == rowDate);
                var status = record?.Status ?? AttendanceStatus.Present;
                var checkIn = record?.CheckInTime ?? e.ShiftTiming?.StartTime;
                var checkOut = record?.CheckOutTime ?? e.ShiftTiming?.EndTime;
                var workedStatus = status is AttendanceStatus.Present or AttendanceStatus.HalfDay
                    or AttendanceStatus.Holiday or AttendanceStatus.WeekOff;

                return new AttendanceEntryRowViewModel
                {
                    EmployeeId = e.Id,
                    Date = rowDate,
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.Name,
                    DepartmentName = e.DepartmentRef?.Name ?? e.Department,
                    ShiftName = e.ShiftTiming?.Name,
                    Status = status,
                    BreakMinutes = e.ShiftTiming?.BreakMinutes ?? 0,
                    // Default a fresh row to the shift's own timings so a full day of
                    // attendance is one click rather than dozens of keystrokes.
                    CheckInTime = checkIn.HasValue ? FormatTime(checkIn.Value) : null,
                    CheckOutTime = checkOut.HasValue ? FormatTime(checkOut.Value) : null,
                    WorkedHours = workedStatus
                        ? record?.WorkedHours > 0
                            ? record.WorkedHours
                            : AttendanceTimeCalculator.WorkedHours(checkIn, checkOut, e.ShiftTiming?.BreakMinutes ?? 0)
                        : 0m,
                    Remarks = record?.Remarks
                };
            })).ToList();
        }

        private static string FormatTime(TimeSpan value) => value.ToString(@"hh\:mm");

        private static bool TryParseTime(string? value, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(value)) return false;

            return TimeSpan.TryParseExact(value, new[] { @"hh\:mm", @"h\:mm", @"hh\:mm\:ss" },
                CultureInfo.InvariantCulture, out result);
        }
    }
}
