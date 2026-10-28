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
    /// <summary>
    /// Cashing in unused leave. The amount is fixed at submission time from
    /// (basic salary / 30) × days, so a later increment doesn't rewrite history.
    /// </summary>
    [Authorize]
    [Route("Payroll/LeaveEncashmentRequests")]
    public class LeaveEncashmentRequestsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaveEncashmentRequestsController(ApplicationDbContext db, IPayrollService payrollService, UserManager<ApplicationUser> userManager)
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
            var list = await _db.LeaveEncashmentRequests
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
                    Detail = l.DaysToEncash + " day(s)",
                    Amount = l.Amount,
                    Status = l.Status.ToString(),
                    CreatedOn = l.CreatedOn
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new LeaveEncashmentRequestFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _db.LeaveEncashmentRequests.FindAsync(id);
            if (request == null) return NotFound();
            if (request.Status != VoucherStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new LeaveEncashmentRequestFormViewModel
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                DaysToEncash = request.DaysToEncash,
                Reason = request.Reason
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(LeaveEncashmentRequestFormViewModel model)
        {
            var employee = await _db.Employees.FindAsync(model.EmployeeId);
            if (employee == null)
                ModelState.AddModelError(nameof(model.EmployeeId), "Select a valid employee.");

            var leaveType = await _db.LeaveTypes.FindAsync(model.LeaveTypeId);
            if (leaveType == null)
                ModelState.AddModelError(nameof(model.LeaveTypeId), "Select a valid leave type.");
            else if (!leaveType.IsEncashable)
                ModelState.AddModelError(nameof(model.LeaveTypeId), $"{leaveType.Name} is not marked as encashable in Payroll Settings.");
            else if (model.DaysToEncash > leaveType.DaysPerYear && leaveType.DaysPerYear > 0)
                ModelState.AddModelError(nameof(model.DaysToEncash), $"{leaveType.Name} allows at most {leaveType.DaysPerYear} days a year.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            LeaveEncashmentRequest request;

            if (isNew)
            {
                request = new LeaveEncashmentRequest
                {
                    CreatedOn = DateTime.UtcNow,
                    RequestNumber = await _payrollService.GenerateNumberAsync("ENC",
                        n => _db.LeaveEncashmentRequests.AnyAsync(l => l.RequestNumber == n))
                };
                _db.LeaveEncashmentRequests.Add(request);
            }
            else
            {
                var existing = await _db.LeaveEncashmentRequests.FindAsync(model.Id);
                if (existing == null) return NotFound();
                if (existing.Status != VoucherStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Only pending requests can be edited.";
                    return RedirectToAction(nameof(Index));
                }
                request = existing;
            }

            request.EmployeeId = model.EmployeeId;
            request.LeaveTypeId = model.LeaveTypeId;
            request.DaysToEncash = model.DaysToEncash;
            // Frozen at submission time — one day equals one thirtieth of basic salary.
            request.Amount = Math.Round((employee!.BasicSalary / 30m) * model.DaysToEncash, 2);
            request.Reason = model.Reason;
            request.Status = VoucherStatus.Pending;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Encashment request {request.RequestNumber} submitted for {request.Amount:N2}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _db.LeaveEncashmentRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending requests can be approved." });

            var user = await _userManager.GetUserAsync(User);
            request.Status = VoucherStatus.Approved;
            request.ApprovedById = user?.Id;
            request.ApprovedByName = user?.FullName;
            request.ApprovedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Encashment {request.RequestNumber} approved for {request.Amount:N2}." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest payload)
        {
            var request = await _db.LeaveEncashmentRequests.FindAsync(payload.Id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending requests can be rejected." });

            var user = await _userManager.GetUserAsync(User);
            request.Status = VoucherStatus.Rejected;
            request.ApprovedById = user?.Id;
            request.ApprovedByName = user?.FullName;
            request.ApprovedOn = DateTime.UtcNow;
            request.RejectionReason = payload.Reason;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Request {request.RequestNumber} rejected." });
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _db.LeaveEncashmentRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending requests can be deleted." });

            _db.LeaveEncashmentRequests.Remove(request);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Request deleted." });
        }

        private async Task PopulateDropdownsAsync(LeaveEncashmentRequestFormViewModel vm)
        {
            vm.Employees = await _db.Employees.Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                .ToListAsync();

            vm.LeaveTypes = await _db.LeaveTypes.Where(l => l.IsActive && l.IsEncashable)
                .OrderBy(l => l.Name)
                .Select(l => new LeaveTypeOption(l.Id, l.Name, l.IsEncashable))
                .ToListAsync();
        }
    }
}
