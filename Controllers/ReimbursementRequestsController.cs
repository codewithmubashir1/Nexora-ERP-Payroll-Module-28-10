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
    [Route("Payroll/ReimbursementRequests")]
    public class ReimbursementRequestsController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IPayrollService _payrollService;
        private readonly IFileUploadService _fileUploadService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReimbursementRequestsController(
            ApplicationDbContext db,
            IPayrollService payrollService,
            IFileUploadService fileUploadService,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _payrollService = payrollService;
            _fileUploadService = fileUploadService;
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
            var list = await _db.ReimbursementRequests
                .Include(r => r.Employee).ThenInclude(e => e!.DepartmentRef)
                .Include(r => r.ReimbursementType)
                .OrderByDescending(r => r.Id)
                .Select(r => new RequestListItemViewModel
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    EmployeeName = r.Employee!.Name,
                    EmployeeCode = r.Employee.EmployeeCode,
                    DepartmentName = r.Employee.DepartmentRef != null ? r.Employee.DepartmentRef.Name : r.Employee.Department,
                    Category = r.ReimbursementType!.Name,
                    Detail = r.ExpenseDate.ToString("dd MMM yyyy"),
                    Amount = r.Amount,
                    Status = r.Status.ToString(),
                    CreatedOn = r.CreatedOn
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new ReimbursementRequestFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _db.ReimbursementRequests.FindAsync(id);
            if (request == null) return NotFound();
            if (request.Status != VoucherStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new ReimbursementRequestFormViewModel
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                EmployeeId = request.EmployeeId,
                ReimbursementTypeId = request.ReimbursementTypeId,
                Amount = request.Amount,
                ExpenseDate = request.ExpenseDate,
                Description = request.Description,
                ExistingReceiptImagePath = request.ReceiptImagePath
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ReimbursementRequestFormViewModel model)
        {
            var type = await _db.ReimbursementTypes.FindAsync(model.ReimbursementTypeId);
            if (type == null)
                ModelState.AddModelError(nameof(model.ReimbursementTypeId), "Select a valid expense type.");
            else if (type.MaxLimit.HasValue && model.Amount > type.MaxLimit.Value)
                ModelState.AddModelError(nameof(model.Amount), $"{type.Name} is capped at {type.MaxLimit.Value:N2} in Payroll Settings.");

            if (model.ExpenseDate.Date > DateTime.Today)
                ModelState.AddModelError(nameof(model.ExpenseDate), "The expense date cannot be in the future.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            ReimbursementRequest request;

            if (isNew)
            {
                request = new ReimbursementRequest
                {
                    CreatedOn = DateTime.UtcNow,
                    RequestNumber = await _payrollService.GenerateNumberAsync("RMB",
                        n => _db.ReimbursementRequests.AnyAsync(r => r.RequestNumber == n))
                };
                _db.ReimbursementRequests.Add(request);
            }
            else
            {
                var existing = await _db.ReimbursementRequests.FindAsync(model.Id);
                if (existing == null) return NotFound();
                if (existing.Status != VoucherStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Only pending requests can be edited.";
                    return RedirectToAction(nameof(Index));
                }
                request = existing;
            }

            request.EmployeeId = model.EmployeeId;
            request.ReimbursementTypeId = model.ReimbursementTypeId;
            request.Amount = model.Amount;
            request.ExpenseDate = model.ExpenseDate.Date;
            request.Description = model.Description;
            request.Status = VoucherStatus.Pending;

            if (model.Receipt != null && model.Receipt.Length > 0)
            {
                try
                {
                    var oldPath = request.ReceiptImagePath;
                    request.ReceiptImagePath = await _fileUploadService.SaveReceiptAsync(model.Receipt, request.RequestNumber);
                    _fileUploadService.DeleteIfExists(oldPath);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(model.Receipt), ex.Message);
                    await PopulateDropdownsAsync(model);
                    return View("Form", model);
                }
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Reimbursement request {request.RequestNumber} submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _db.ReimbursementRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending) return Json(new { success = false, message = "Only pending requests can be approved." });

            var user = await _userManager.GetUserAsync(User);
            request.Status = VoucherStatus.Approved;
            request.ApprovedById = user?.Id;
            request.ApprovedByName = user?.FullName;
            request.ApprovedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Reimbursement {request.RequestNumber} approved for {request.Amount:N2}." });
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> Reject([FromBody] RejectRequest payload)
        {
            var request = await _db.ReimbursementRequests.FindAsync(payload.Id);
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
            var request = await _db.ReimbursementRequests.FindAsync(id);
            if (request == null) return Json(new { success = false, message = "Request not found." });
            if (request.Status != VoucherStatus.Pending)
                return Json(new { success = false, message = "Only pending requests can be deleted." });

            _fileUploadService.DeleteIfExists(request.ReceiptImagePath);
            _db.ReimbursementRequests.Remove(request);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Request deleted." });
        }

        private async Task PopulateDropdownsAsync(ReimbursementRequestFormViewModel vm)
        {
            vm.Employees = await _db.Employees.Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new PayrollEmployeeOption(e.Id, e.Name, e.EmployeeCode, e.BasicSalary))
                .ToListAsync();

            vm.ReimbursementTypes = await _db.ReimbursementTypes.Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .Select(r => new ReimbursementTypeOption(r.Id, r.Name, r.MaxLimit))
                .ToListAsync();
        }
    }
}
