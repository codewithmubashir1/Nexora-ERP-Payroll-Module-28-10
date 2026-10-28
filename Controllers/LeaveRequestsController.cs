using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Payroll/LeaveRequests")]
    public class LeaveRequestsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaveRequestsController(ApplicationDbContext db, IPayrollService payrollService, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _payrollService = payrollService;
            _userManager = userManager;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.CanApprove = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Accountant);
            return View();
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.LeaveRequests
                .Include(l => l.Employee).ThenInclude(e => e!.DepartmentRef)
                .Include(l => l.LeaveType)
                .OrderByDescending(l => l.Id)
                .Select(l => new RequestListItemViewModel
                {
                    Id = l.Id,
                    RequestNumber = l.RequestNumber,
                    EmployeeName = l.Employee!.Name,
                    EmployeeCode = l.Employee.EmployeeCode,
                    DepartmentName = l.Employee.DepartmentRef != null ? l.Employee.DepartmentRef.Name : l.Employee.Department,
                    Category = l.LeaveType!.Name,
                    Detail = l.FromDate.ToString("dd MMM") + " – " + l.ToDate.ToString("dd MMM yyyy") + " (" + l.TotalDays + " day(s))",
                    Amount = 0,
                    Status = l.Status.ToString(),
                    CreatedOn = l.CreatedOn
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new LeaveRequestFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _db.LeaveRequests.FindAsync(id);
            if (request == null) return NotFound();
            if (request.Status != VoucherStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending leave requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new LeaveRequestFormViewModel
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Reason = request.Reason
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(LeaveRequestFormViewModel model)
        {
            if (model.ToDate.Date < model.FromDate.Date)
                ModelState.AddModelError(nameof(model.ToDate), "The end date cannot be before the start date.");

            var totalDays = (model.ToDate.Date - model.FromDate.Date).Days + 1;

            // Overlapping leave for the same employee is almost always a mistake.
            var overlaps = await _db.LeaveRequests.AnyAsync(l =>
                l.EmployeeId == model.EmployeeId &&
                l.Id != model.Id &&
                l.Status != VoucherStatus.Rejected &&
                l.FromDate <= model.ToDate.Date &&
                l.ToDate >= model.FromDate.Date);

            if (overlaps)
                ModelState.AddModelError(nameof(model.FromDate), "This employee already has leave booked that overlaps these dates.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            LeaveRequest request;

            if (isNew)
            {
                request = new LeaveRequest
                {
                    CreatedOn = DateTime.UtcNow,
                    RequestNumber = await _payrollService.GenerateNumberAsync("LV",
                        n => _db.LeaveRequests.AnyAsync(l => l.RequestNumber == n))
                };
                _db.LeaveRequests.Add(request);
            }
            else
            {
                var existing = await _db.LeaveRequests.FindAsync(model.Id);
                if (existing == null) return NotFound();
                if (existing.Status != VoucherStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Only pending leave requests can be edited.";
                    return RedirectToAction(nameof(Index));
                }
                request = existing;
            }

            request.EmployeeId = model.EmployeeId;
            request.LeaveTypeId = model.LeaveTypeId;
            request.FromDate = model.FromDate.Date;
            request.ToDate = model.ToDate.Date;
            request.TotalDays = totalDays;
            request.Reason = model.Reason;
            request.Status = VoucherStatus.Pending;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Leave request {request.RequestNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _db.LeaveRequests.Include(l => l.LeaveType).FirstOrDefaultAsync(l => l.Id == id);
            if (request == null) return Json(new { success = false, message = "Leave request not found." });
            if (request.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending requests can be approved." });

            var user = await _userManager.GetUserAsync(User);
            request.Status = VoucherStatus.Approved;
            request.ApprovedById = user?.Id;
            request.ApprovedByName = user?.FullName;
            request.ApprovedOn = DateTime.UtcNow;

            // Approving leave writes the attendance days straight away, so the salary
            // sheet reflects it without anyone re-keying the same dates.
            await MarkAttendanceAsLeaveAsync(request);

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Leave request {request.RequestNumber} approved and posted to attendance." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest payload)
        {
            var request = await _db.LeaveRequests.FindAsync(payload.Id);
            if (request == null) return Json(new { success = false, message = "Leave request not found." });
            if (request.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending requests can be rejected." });

            var user = await _userManager.GetUserAsync(User);
            request.Status = VoucherStatus.Rejected;
            request.ApprovedById = user?.Id;
            request.ApprovedByName = user?.FullName;
            request.ApprovedOn = DateTime.UtcNow;
            request.RejectionReason = payload.Reason;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Leave request {request.RequestNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _db.LeaveRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Leave request not found." });
            if (request.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending requests can be deleted." });

            _db.LeaveRequests.Remove(request);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Leave request deleted." });
        }

        private async Task MarkAttendanceAsLeaveAsync(LeaveRequest request)
        {
            var dates = new List<DateTime>();
            for (var d = request.FromDate.Date; d <= request.ToDate.Date; d = d.AddDays(1)) dates.Add(d);

            var existing = await _db.AttendanceRecords
                .Where(a => a.EmployeeId == request.EmployeeId && dates.Contains(a.Date))
                .ToListAsync();

            foreach (var date in dates)
            {
                var record = existing.FirstOrDefault(a => a.Date == date);
                if (record == null)
                {
                    record = new AttendanceRecord
                    {
                        EmployeeId = request.EmployeeId,
                        Date = date,
                        CreatedOn = DateTime.UtcNow
                    };
                    _db.AttendanceRecords.Add(record);
                }

                record.Status = AttendanceStatus.Leave;
                record.CheckInTime = null;
                record.CheckOutTime = null;
                record.IsLate = false;
                record.LateByMinutes = 0;
                record.IsEarlyDeparture = false;
                record.EarlyByMinutes = 0;
                record.Remarks = $"{request.LeaveType?.Name ?? "Leave"} — {request.RequestNumber}";
            }
        }

        private async Task PopulateDropdownsAsync(LeaveRequestFormViewModel vm)
        {
            vm.Employees = await _db.Employees.Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                .ToListAsync();

            vm.LeaveTypes = await _db.LeaveTypes.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LeaveTypeOption(l.Id, l.Name, l.IsEncashable))
                .ToListAsync();
        }
    }
}
