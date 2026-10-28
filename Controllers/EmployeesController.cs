using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.ViewModels.Payroll;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Text;
using System.Globalization;
using System.Xml;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Accounts/Employees")]
    public class EmployeesController : Controller
    {
        private const string EditorRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;
        private readonly IFileUploadService _fileUploadService;
        private readonly IPayrollService _payrollService;
        private readonly ICompanyInfoService _companyInfoService;

        public EmployeesController(ApplicationDbContext db, IFileUploadService fileUploadService, IPayrollService payrollService, ICompanyInfoService companyInfoService)
        {
            _db = db;
            _fileUploadService = fileUploadService;
            _payrollService = payrollService;
            _companyInfoService = companyInfoService;
        }

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Employees
                .Include(e => e.DepartmentRef)
                .Include(e => e.ShiftTiming)
                .OrderByDescending(e => e.Id)
                .Select(e => new PayrollEmployeeListItemViewModel
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    Name = e.Name,
                    ProfileImagePath = e.ProfileImagePath,
                    DepartmentName = e.DepartmentRef != null ? e.DepartmentRef.Name : e.Department,
                    Designation = e.Designation,
                    ShiftName = e.ShiftTiming != null ? e.ShiftTiming.Name : null,
                    CNICNumber = e.CNICNumber,
                    Contact = ((e.ContactCountryCode ?? "") + " " + (e.Phone ?? "")).Trim(),
                    BasicSalary = e.BasicSalary,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export()
        {
            var employees = await _db.Employees
                .Include(e => e.DepartmentRef)
                .Include(e => e.ShiftTiming)
                .OrderBy(e => e.EmployeeCode)
                .ToListAsync();
            var headers = new[]
            {
                "EmployeeCode", "Name", "Department", "Designation", "CNICNumber", "BloodGroup",
                "ContactCountryCode", "Phone", "Email", "Address", "ZipCode", "ShiftName",
                "BasicSalary", "HouseRentAllowance", "MedicalAllowance", "OtherAllowances",
                "BankName", "BankAccountNumber", "TaxNumber", "EOBINumber", "JoiningDate", "IsActive"
            };
            var rows = employees.Select(e => new string?[]
            {
                e.EmployeeCode, e.Name, e.DepartmentRef?.Name ?? e.Department, e.Designation, e.CNICNumber,
                e.BloodGroup, e.ContactCountryCode, e.Phone, e.Email, e.Address, e.ZipCode, e.ShiftTiming?.Name,
                e.BasicSalary.ToString(CultureInfo.InvariantCulture), e.HouseRentAllowance.ToString(CultureInfo.InvariantCulture),
                e.MedicalAllowance.ToString(CultureInfo.InvariantCulture), e.OtherAllowances.ToString(CultureInfo.InvariantCulture),
                e.BankName, e.BankAccountNumber, e.TaxNumber, e.EOBINumber, e.JoiningDate?.ToString("yyyy-MM-dd"),
                e.IsActive.ToString()
            });

            return File(CsvUtility.WriteWithHeader(headers, rows), "text/csv", "employees.csv");
        }

        [HttpGet("ImportTemplate")]
        public IActionResult ImportTemplate()
        {
            var headers = new[]
            {
                "EmployeeCode", "Name", "Department", "Designation", "CNICNumber", "BloodGroup",
                "ContactCountryCode", "Phone", "Email", "Address", "ZipCode", "ShiftName",
                "BasicSalary", "HouseRentAllowance", "MedicalAllowance", "OtherAllowances",
                "BankName", "BankAccountNumber", "TaxNumber", "EOBINumber", "JoiningDate", "IsActive"
            };
            return File(CsvUtility.WriteWithHeader(headers, Array.Empty<IEnumerable<string?>>()), "text/csv", "employees_template.csv");
        }

        [HttpPost("Import")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EditorRoles)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Choose a non-empty CSV file." });

            var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
            var errors = new List<string>();
            var departments = await _db.Departments.ToListAsync();
            var shifts = await _db.ShiftTimings.ToListAsync();
            var employees = await _db.Employees.ToListAsync();
            var created = 0;
            var updated = 0;

            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                var rowNumber = index + 2;
                var name = row.Get("Name");
                var code = row.Get("EmployeeCode");
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add($"Row {rowNumber}: Name is required.");
                    continue;
                }

                var cnic = row.Get("CNICNumber");
                var employee = !string.IsNullOrWhiteSpace(code)
                    ? employees.FirstOrDefault(e => e.EmployeeCode.Equals(code, StringComparison.OrdinalIgnoreCase))
                    : !string.IsNullOrWhiteSpace(cnic) ? employees.FirstOrDefault(e => e.CNICNumber == cnic) : null;
                var isNew = employee == null;

                if (!TryCsvDecimal(row.Get("BasicSalary"), out var basicSalary) ||
                    !TryCsvDecimal(row.Get("HouseRentAllowance"), out var houseRent) ||
                    !TryCsvDecimal(row.Get("MedicalAllowance"), out var medical) ||
                    !TryCsvDecimal(row.Get("OtherAllowances"), out var other))
                {
                    errors.Add($"Row {rowNumber}: Salary and allowance values must be valid numbers.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(cnic) && !System.Text.RegularExpressions.Regex.IsMatch(cnic, @"^\d{5}-\d{7}-\d$"))
                {
                    errors.Add($"Row {rowNumber}: CNIC must use 00000-0000000-0 format.");
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(cnic) && employees.Any(e => e.Id != (employee?.Id ?? 0) && e.CNICNumber == cnic))
                {
                    errors.Add($"Row {rowNumber}: CNIC is already assigned to another employee.");
                    continue;
                }

                if (!isNew && employees.Any(e => e.Id != employee!.Id && !string.IsNullOrWhiteSpace(code) && e.EmployeeCode.Equals(code, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add($"Row {rowNumber}: Employee code is already in use.");
                    continue;
                }

                employee ??= new Employee { CreatedOn = DateTime.UtcNow };
                var departmentName = row.Get("Department");
                var department = string.IsNullOrWhiteSpace(departmentName)
                    ? null
                    : departments.FirstOrDefault(d => d.Name.Equals(departmentName, StringComparison.OrdinalIgnoreCase) || string.Equals(d.Code, departmentName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(departmentName) && department == null)
                {
                    errors.Add($"Row {rowNumber}: Department '{departmentName}' does not exist.");
                    continue;
                }
                var shiftName = row.Get("ShiftName");
                var shift = string.IsNullOrWhiteSpace(shiftName)
                    ? null
                    : shifts.FirstOrDefault(s => s.Name.Equals(shiftName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(shiftName) && shift == null)
                {
                    errors.Add($"Row {rowNumber}: Shift '{shiftName}' does not exist.");
                    continue;
                }

                var joiningText = row.Get("JoiningDate");
                if (!string.IsNullOrWhiteSpace(joiningText) && !TryCsvDate(joiningText, out _))
                {
                    errors.Add($"Row {rowNumber}: JoiningDate must use yyyy-MM-dd or dd/MM/yyyy format.");
                    continue;
                }
                var activeText = row.Get("IsActive");
                if (!string.IsNullOrWhiteSpace(activeText) && !bool.TryParse(activeText, out _))
                {
                    errors.Add($"Row {rowNumber}: IsActive must be true or false.");
                    continue;
                }

                if (isNew)
                {
                    employee.EmployeeCode = string.IsNullOrWhiteSpace(code) ? await GenerateEmployeeCodeAsync() : code;
                    if (employees.Any(e => e.EmployeeCode.Equals(employee.EmployeeCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        errors.Add($"Row {rowNumber}: Employee code '{employee.EmployeeCode}' is already in use.");
                        continue;
                    }
                    _db.Employees.Add(employee);
                    employees.Add(employee);
                    created++;
                }
                else
                {
                    updated++;
                }

                employee.Name = name;
                employee.DepartmentId = string.IsNullOrWhiteSpace(departmentName) ? employee.DepartmentId : department!.Id;
                employee.Department = department?.Name ?? departmentName ?? employee.Department;
                employee.Designation = row.Get("Designation") ?? employee.Designation;
                employee.CNICNumber = cnic ?? employee.CNICNumber;
                employee.BloodGroup = row.Get("BloodGroup") ?? employee.BloodGroup;
                employee.ContactCountryCode = row.Get("ContactCountryCode") ?? employee.ContactCountryCode ?? "+92";
                employee.Phone = row.Get("Phone") ?? employee.Phone;
                employee.Email = row.Get("Email") ?? employee.Email;
                employee.Address = row.Get("Address") ?? employee.Address;
                employee.ZipCode = row.Get("ZipCode") ?? employee.ZipCode;
                employee.ShiftTimingId = shift?.Id ?? employee.ShiftTimingId;
                var oldBasicSalary = employee.BasicSalary;
                employee.BasicSalary = string.IsNullOrWhiteSpace(row.Get("BasicSalary")) ? employee.BasicSalary : basicSalary;
                employee.HouseRentAllowance = string.IsNullOrWhiteSpace(row.Get("HouseRentAllowance")) ? employee.HouseRentAllowance : houseRent;
                employee.MedicalAllowance = string.IsNullOrWhiteSpace(row.Get("MedicalAllowance")) ? employee.MedicalAllowance : medical;
                employee.OtherAllowances = string.IsNullOrWhiteSpace(row.Get("OtherAllowances")) ? employee.OtherAllowances : other;
                employee.MonthlySalary = employee.BasicSalary + employee.HouseRentAllowance + employee.MedicalAllowance + employee.OtherAllowances;
                employee.BankName = row.Get("BankName") ?? employee.BankName;
                employee.BankAccountNumber = row.Get("BankAccountNumber") ?? employee.BankAccountNumber;
                employee.TaxNumber = row.Get("TaxNumber") ?? employee.TaxNumber;
                employee.EOBINumber = row.Get("EOBINumber") ?? employee.EOBINumber;
                if (TryCsvDate(joiningText, out var joiningDate)) employee.JoiningDate = joiningDate;
                if (bool.TryParse(activeText, out var isActive)) employee.IsActive = isActive;
                if (!isNew && employee.BasicSalary != oldBasicSalary)
                {
                    _db.SalaryIncrements.Add(new SalaryIncrement
                    {
                        EmployeeId = employee.Id,
                        EffectiveDate = DateTime.Today,
                        OldBasicSalary = oldBasicSalary,
                        NewBasicSalary = employee.BasicSalary,
                        Reason = "Salary changed via employee CSV import.",
                        ApprovedByName = User.Identity?.Name,
                        CreatedOn = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();
            var summary = $"Imported {created} employee(s); updated {updated}.";
            return Json(new { success = errors.Count == 0, errorCount = errors.Count, summary, errors });
        }

        /// <summary>Feeds the employee pickers on request, payslip and increment forms.</summary>
        [HttpGet("GetBasics/{id:int}")]
        public async Task<IActionResult> GetBasics(int id)
        {
            var employee = await _db.Employees
                .Include(e => e.ShiftTiming)
                .Include(e => e.DepartmentRef)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            var outstanding = await _payrollService.GetOutstandingAdvanceAsync(id);

            return Json(new
            {
                id = employee.Id,
                name = employee.Name,
                employeeCode = employee.EmployeeCode,
                basicSalary = employee.BasicSalary,
                grossSalary = employee.BasicSalary + employee.HouseRentAllowance + employee.MedicalAllowance + employee.OtherAllowances,
                department = employee.DepartmentRef?.Name,
                shift = employee.ShiftTiming?.Name,
                shiftHours = employee.ShiftTiming?.ShiftHours ?? 8m,
                dailyRate = Math.Round(employee.BasicSalary / 30m, 2),
                outstandingAdvance = outstanding
            });
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _db.Employees
                .Include(e => e.DepartmentRef)
                .Include(e => e.ShiftTiming)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            var vm = new EmployeeDetailViewModel
            {
                Employee = employee,
                DepartmentName = employee.DepartmentRef?.Name ?? employee.Department,
                ShiftName = employee.ShiftTiming?.Name,
                ShiftHours = employee.ShiftTiming?.ShiftHours ?? 8m,
                OutstandingAdvance = await _payrollService.GetOutstandingAdvanceAsync(id),
                Increments = await _db.SalaryIncrements
                    .Where(s => s.EmployeeId == id)
                    .OrderByDescending(s => s.EffectiveDate)
                    .Take(10)
                    .ToListAsync(),
                RecentPayslips = await _db.Payslips
                    .Where(p => p.EmployeeId == id)
                    .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                    .Take(12)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpGet("Card/{id:int}")]
        public async Task<IActionResult> Card(int id, string orientation = "landscape")
        {
            var employee = await _db.Employees
                .Include(e => e.DepartmentRef)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();

            ViewBag.DepartmentName = employee.DepartmentRef?.Name ?? employee.Department;
            ViewBag.Company = await _companyInfoService.GetAsync();
            ViewBag.Orientation = string.Equals(orientation, "portrait", StringComparison.OrdinalIgnoreCase)
                ? "portrait"
                : "landscape";
            var employeeCardUrl = Url.Action(nameof(Details), "Employees", new { id }, Request.Scheme) ?? string.Empty;
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(employeeCardUrl, QRCodeGenerator.ECCLevel.M);
            var qrCode = new PngByteQRCode(qrData);
            ViewBag.EmployeeQrDataUri = $"data:image/png;base64,{Convert.ToBase64String(qrCode.GetGraphic(10))}";
            return View(employee);
        }

        [HttpGet("Card/{id:int}/Excel")]
        public async Task<IActionResult> ExportCardExcel(int id)
        {
            var employee = await _db.Employees
                .Include(e => e.DepartmentRef)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();

            var company = await _companyInfoService.GetAsync();

            var rows = new (string Label, string Value)[]
            {
                ("Company", company.CompanyName),
                ("Company Address", company.Address ?? string.Empty),
                ("Employee Code", employee.EmployeeCode),
                ("Full Name", employee.Name),
                ("CNIC", employee.CNICNumber ?? string.Empty),
                ("Designation", employee.Designation ?? string.Empty),
                ("Department", employee.DepartmentRef?.Name ?? employee.Department ?? string.Empty),
                ("Blood Group", employee.BloodGroup ?? string.Empty),
                ("Phone", $"{employee.ContactCountryCode} {employee.Phone}".Trim())
            };

            const string spreadsheetNamespace = "urn:schemas-microsoft-com:office:spreadsheet";
            using var output = new StringWriter();
            using (var writer = XmlWriter.Create(output, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("Workbook", spreadsheetNamespace);
                writer.WriteAttributeString("xmlns", "ss", null, spreadsheetNamespace);
                writer.WriteStartElement("Worksheet", spreadsheetNamespace);
                writer.WriteAttributeString("ss", "Name", spreadsheetNamespace, "Employee Card");
                writer.WriteStartElement("Table", spreadsheetNamespace);
                foreach (var row in rows)
                {
                    writer.WriteStartElement("Row", spreadsheetNamespace);
                    WriteExcelCell(writer, spreadsheetNamespace, row.Label);
                    WriteExcelCell(writer, spreadsheetNamespace, row.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(output.ToString())).ToArray();
            return File(bytes, "application/vnd.ms-excel", $"{employee.EmployeeCode}-employee-card.xls");
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new PayrollEmployeeFormViewModel { JoiningDate = DateTime.Today };
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var vm = new PayrollEmployeeFormViewModel
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                Name = employee.Name,
                DepartmentId = employee.DepartmentId,
                Designation = employee.Designation,
                CNICNumber = employee.CNICNumber,
                BloodGroup = employee.BloodGroup,
                ContactCountryCode = string.IsNullOrWhiteSpace(employee.ContactCountryCode) ? "+92" : employee.ContactCountryCode,
                Phone = employee.Phone,
                Email = employee.Email,
                Address = employee.Address,
                ZipCode = employee.ZipCode,
                ExistingProfileImagePath = employee.ProfileImagePath,
                ShiftTimingId = employee.ShiftTimingId,
                BasicSalary = employee.BasicSalary,
                HouseRentAllowance = employee.HouseRentAllowance,
                MedicalAllowance = employee.MedicalAllowance,
                OtherAllowances = employee.OtherAllowances,
                BankName = employee.BankName,
                BankAccountNumber = employee.BankAccountNumber,
                TaxNumber = employee.TaxNumber,
                EOBINumber = employee.EOBINumber,
                JoiningDate = employee.JoiningDate,
                IsActive = employee.IsActive
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = EditorRoles)]
        public async Task<IActionResult> Save(PayrollEmployeeFormViewModel model)
        {
            // CNIC is the one natural key an HR team relies on, so it must stay unique.
            if (!string.IsNullOrWhiteSpace(model.CNICNumber))
            {
                var duplicate = await _db.Employees
                    .AnyAsync(e => e.CNICNumber == model.CNICNumber && e.Id != model.Id);
                if (duplicate)
                    ModelState.AddModelError(nameof(model.CNICNumber), "Another employee already has this CNIC number.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            Employee employee;

            if (isNew)
            {
                employee = new Employee { CreatedOn = DateTime.UtcNow, EmployeeCode = await GenerateEmployeeCodeAsync() };
                _db.Employees.Add(employee);
            }
            else
            {
                var existing = await _db.Employees.FindAsync(model.Id);
                if (existing == null) return NotFound();
                employee = existing;
            }

            // Changing basic salary on an existing employee writes an increment record,
            // so the salary history is never silently lost.
            var salaryChanged = !isNew && employee.BasicSalary != model.BasicSalary;
            var previousBasic = employee.BasicSalary;

            var departmentName = model.DepartmentId.HasValue
                ? await _db.Departments.Where(d => d.Id == model.DepartmentId).Select(d => d.Name).FirstOrDefaultAsync()
                : null;

            employee.Name = model.Name;
            employee.DepartmentId = model.DepartmentId;
            employee.Department = departmentName ?? employee.Department;
            employee.Designation = model.Designation;
            employee.CNICNumber = model.CNICNumber;
            employee.BloodGroup = model.BloodGroup;
            employee.ContactCountryCode = model.ContactCountryCode;
            employee.Phone = model.Phone;
            employee.Email = model.Email;
            employee.Address = model.Address;
            employee.ZipCode = model.ZipCode;
            employee.ShiftTimingId = model.ShiftTimingId;
            employee.BasicSalary = model.BasicSalary;
            employee.HouseRentAllowance = model.HouseRentAllowance;
            employee.MedicalAllowance = model.MedicalAllowance;
            employee.OtherAllowances = model.OtherAllowances;
            employee.MonthlySalary = model.GrossSalary;
            employee.BankName = model.BankName;
            employee.BankAccountNumber = model.BankAccountNumber;
            employee.TaxNumber = model.TaxNumber;
            employee.EOBINumber = model.EOBINumber;
            employee.JoiningDate = model.JoiningDate;
            employee.IsActive = model.IsActive;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                try
                {
                    var oldPath = employee.ProfileImagePath;
                    employee.ProfileImagePath = await _fileUploadService.SaveAvatarAsync(model.ProfileImage, employee.EmployeeCode);
                    _fileUploadService.DeleteIfExists(oldPath);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(model.ProfileImage), ex.Message);
                    await PopulateDropdownsAsync(model);
                    return View("Form", model);
                }
            }

            await _db.SaveChangesAsync();

            if (salaryChanged)
            {
                _db.SalaryIncrements.Add(new SalaryIncrement
                {
                    EmployeeId = employee.Id,
                    EffectiveDate = DateTime.Today,
                    OldBasicSalary = previousBasic,
                    NewBasicSalary = model.BasicSalary,
                    Reason = "Basic salary updated on the employee record.",
                    ApprovedByName = User.Identity?.Name,
                    CreatedOn = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = isNew
                ? $"Employee {employee.EmployeeCode} created successfully."
                : "Employee updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee == null) return Json(new { success = false, message = "Employee not found." });

            var isReferenced =
                await _db.CashPaymentVouchers.AnyAsync(v => v.PartyType == PartyType.Employee && v.PartyId == id) ||
                await _db.BankPaymentVouchers.AnyAsync(v => v.PartyType == PartyType.Employee && v.PartyId == id) ||
                await _db.CashReceiptVouchers.AnyAsync(v => v.PartyType == PartyType.Employee && v.PartyId == id) ||
                await _db.BankReceiptVouchers.AnyAsync(v => v.PartyType == PartyType.Employee && v.PartyId == id) ||
                await _db.Payslips.AnyAsync(p => p.EmployeeId == id) ||
                await _db.AttendanceRecords.AnyAsync(a => a.EmployeeId == id) ||
                await _db.LeaveRequests.AnyAsync(l => l.EmployeeId == id) ||
                await _db.LoanAdvanceRequests.AnyAsync(l => l.EmployeeId == id) ||
                await _db.Departments.AnyAsync(d => d.HeadEmployeeId == id);

            if (isReferenced)
                return Json(new { success = false, message = "This employee has payroll or voucher activity and cannot be deleted. Deactivate them instead." });

            _fileUploadService.DeleteIfExists(employee.ProfileImagePath);
            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Employee deleted." });
        }

        private async Task<string> GenerateEmployeeCodeAsync()
        {
            var count = await _db.Employees.CountAsync();
            var candidate = $"EMP-{(count + 1):D6}";
            while (await _db.Employees.AnyAsync(e => e.EmployeeCode == candidate))
            {
                count++;
                candidate = $"EMP-{(count + 1):D6}";
            }
            return candidate;
        }

        private static bool TryCsvDecimal(string? value, out decimal result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0m;
                return true;
            }

            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryCsvDate(string? value, out DateTime result)
        {
            return DateTime.TryParseExact(value, new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        private async Task PopulateDropdownsAsync(PayrollEmployeeFormViewModel vm)
        {
            vm.Departments = await _db.Departments.Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentOption(d.Id, d.Name, d.Code))
                .ToListAsync();

            // ShiftHours is [NotMapped], so the shifts are materialised before projecting.
            var shifts = await _db.ShiftTimings.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            vm.Shifts = shifts.Select(s => new ShiftOption(s.Id, s.Name, s.ShiftHours)).ToList();
        }

        private static void WriteExcelCell(XmlWriter writer, string spreadsheetNamespace, string value)
        {
            writer.WriteStartElement("Cell", spreadsheetNamespace);
            writer.WriteStartElement("Data", spreadsheetNamespace);
            writer.WriteAttributeString("ss", "Type", spreadsheetNamespace, "String");
            writer.WriteString(value);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
