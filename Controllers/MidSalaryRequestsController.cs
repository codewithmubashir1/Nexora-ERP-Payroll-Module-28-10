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
    /// Mid-month salary draws. An approved amount is treated as an advance against
    /// the month's pay and recovered in full on that month's payslip.
    /// </summary>
    [Authorize]
    [Route("Payroll/MidSalaryRequests")]
    public class MidSalaryRequestsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MidSalaryRequestsController(ApplicationDbContext db, IPayrollService payrollService, UserManager<ApplicationUser> userManager)
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
            var list = await _db.MidSalaryRequests
                .Include(m => m.Employee).ThenInclude(e => e!.DepartmentRef)
                .OrderByDescending(m => m.Id)
                .Select(m => new RequestListItemViewModel
                {
                    Id = m.Id,
                    RequestNumber = m.RequestNumber,
                    EmployeeName = m.Employee!.Name,
                    EmployeeCode = m.Employee.EmployeeCode,
                    DepartmentName = m.Employee.DepartmentRef != null ? m.Employee.DepartmentRef.Name : m.Employee.Department,
                    Category = "Mid Salary",
                    Detail = m.Reason,
                    Amount = m.Amount,
                    Status = m.Status.ToString(),
                    CreatedOn = m.CreatedOn
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new MidSalaryRequestFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _db.MidSalaryRequests.FindAsync(id);
            if (request == null) return NotFound();
            if (request.Status != VoucherStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new MidSalaryRequestFormViewModel
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                EmployeeId = request.EmployeeId,
                Amount = request.Amount,
                Reason = request.Reason
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(MidSalaryRequestFormViewModel model)
        {
            var employee = await _db.Employees.FindAsync(model.EmployeeId);
            if (employee == null)
                ModelState.AddModelError(nameof(model.EmployeeId), "Select a valid employee.");
            else if (model.Amount > employee.BasicSalary)
                ModelState.AddModelError(nameof(model.Amount), "A mid-salary draw cannot exceed the employee's basic salary.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            MidSalaryRequest request;

            if (isNew)
            {
                request = new MidSalaryRequest
                {
                    CreatedOn = DateTime.UtcNow,
                    RequestNumber = await _payrollService.GenerateNumberAsync("MID",
                        n => _db.MidSalaryRequests.AnyAsync(m => m.RequestNumber == n))
                };
                _db.MidSalaryRequests.Add(request);
            }
            else
            {
                var existing = await _db.MidSalaryRequests.FindAsync(model.Id);
                if (existing == null) return NotFound();
                if (existing.Status != VoucherStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Only pending requests can be edited.";
                    return RedirectToAction(nameof(Index));
                }
                request = existing;
            }

            request.EmployeeId = model.EmployeeId;
            request.Amount = model.Amount;
            request.Reason = model.Reason;
            request.Status = VoucherStatus.Pending;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Mid-salary request {request.RequestNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _db.MidSalaryRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending requests can be approved." });

            var user = await _userManager.GetUserAsync(User);
            request.Status = VoucherStatus.Approved;
            request.ApprovedById = user?.Id;
            request.ApprovedByName = user?.FullName;
            request.ApprovedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Mid-salary {request.RequestNumber} approved. It will be recovered on this month's payslip." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest payload)
        {
            var request = await _db.MidSalaryRequests.FindAsync(payload.Id);
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
            var request = await _db.MidSalaryRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending requests can be deleted." });

            _db.MidSalaryRequests.Remove(request);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Request deleted." });
        }

        private async Task PopulateDropdownsAsync(MidSalaryRequestFormViewModel vm)
        {
            vm.Employees = await _db.Employees.Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                .ToListAsync();

            if (vm.EmployeeId > 0)
            {
                var basic = vm.Employees.FirstOrDefault(e => e.Id == vm.EmployeeId)?.BasicSalary ?? 0;
                vm.SuggestedMaximum = Math.Round(basic / 2m, 2);
            }
        }
    }
}
