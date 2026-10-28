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
    /// Loans and advances. Once approved, the amount flows into the next payslip as
    /// "current advance" and is recovered over the requested number of months.
    /// </summary>
    [Authorize]
    [Route("Payroll/LoanAdvanceRequests")]
    public class LoanAdvanceRequestsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoanAdvanceRequestsController(ApplicationDbContext db, IPayrollService payrollService, UserManager<ApplicationUser> userManager)
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
            var list = await _db.LoanAdvanceRequests
                .Include(l => l.Employee).ThenInclude(e => e!.DepartmentRef)
                .OrderByDescending(l => l.Id)
                .Select(l => new RequestListItemViewModel
                {
                    Id = l.Id,
                    RequestNumber = l.RequestNumber,
                    EmployeeName = l.Employee!.Name,
                    EmployeeCode = l.Employee.EmployeeCode,
                    DepartmentName = l.Employee.DepartmentRef != null ? l.Employee.DepartmentRef.Name : l.Employee.Department,
                    Category = l.RequestType.ToString(),
                    Detail = l.InstallmentMonths + " month(s)",
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
            var vm = new LoanAdvanceRequestFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _db.LoanAdvanceRequests.FindAsync(id);
            if (request == null) return NotFound();
            if (request.Status != VoucherStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new LoanAdvanceRequestFormViewModel
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                EmployeeId = request.EmployeeId,
                RequestType = request.RequestType,
                Amount = request.Amount,
                InstallmentMonths = request.InstallmentMonths,
                Reason = request.Reason
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(LoanAdvanceRequestFormViewModel model)
        {
            // A simple advance is settled from the next payslip, so it can't be spread.
            if (model.RequestType == LoanAdvanceType.Advance && model.InstallmentMonths > 1)
                ModelState.AddModelError(nameof(model.InstallmentMonths), "An advance is recovered in full from the next salary. Choose 'Loan' to spread it over months.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            LoanAdvanceRequest request;

            if (isNew)
            {
                request = new LoanAdvanceRequest
                {
                    CreatedOn = DateTime.UtcNow,
                    RequestNumber = await _payrollService.GenerateNumberAsync("ADV",
                        n => _db.LoanAdvanceRequests.AnyAsync(l => l.RequestNumber == n))
                };
                _db.LoanAdvanceRequests.Add(request);
            }
            else
            {
                var existing = await _db.LoanAdvanceRequests.FindAsync(model.Id);
                if (existing == null) return NotFound();
                if (existing.Status != VoucherStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Only pending requests can be edited.";
                    return RedirectToAction(nameof(Index));
                }
                request = existing;
            }

            request.EmployeeId = model.EmployeeId;
            request.RequestType = model.RequestType;
            request.Amount = model.Amount;
            request.InstallmentMonths = model.RequestType == LoanAdvanceType.Advance ? 1 : Math.Max(1, model.InstallmentMonths);
            request.Reason = model.Reason;
            request.Status = VoucherStatus.Pending;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{request.RequestType} request {request.RequestNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _db.LoanAdvanceRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending requests can be approved." });

            var user = await _userManager.GetUserAsync(User);
            request.Status = VoucherStatus.Approved;
            request.ApprovedById = user?.Id;
            request.ApprovedByName = user?.FullName;
            request.ApprovedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"{request.RequestType} {request.RequestNumber} approved. It will appear on the next payslip." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest payload)
        {
            var request = await _db.LoanAdvanceRequests.FindAsync(payload.Id);
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
            var request = await _db.LoanAdvanceRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending requests can be deleted." });

            _db.LoanAdvanceRequests.Remove(request);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Request deleted." });
        }

        private async Task PopulateDropdownsAsync(LoanAdvanceRequestFormViewModel vm)
        {
            vm.Employees = await _db.Employees.Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                .ToListAsync();
        }
    }
}
