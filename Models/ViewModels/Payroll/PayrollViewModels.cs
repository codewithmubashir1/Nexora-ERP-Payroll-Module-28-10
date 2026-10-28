using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Payroll;

namespace ERP.Identity.Models.ViewModels.Payroll
{
    // ---------------- Shared option records ----------------

    public record DepartmentOption(int Id, string Name, string? Code);
    public record ShiftOption(int Id, string Name, decimal Hours);
    public record LeaveTypeOption(int Id, string Name, bool IsEncashable);
    public record ReimbursementTypeOption(int Id, string Name, decimal? MaxLimit);
    public record PayrollEmployeeOption(int Id, string Name, string EmployeeCode, decimal BasicSalary);

    /// <summary>Country dial codes for the contact-number picker. Kept server-side so the same list is used on every form.</summary>
    public record CountryDialCode(string Code, string Country);

    public static class DialCodes
    {
        public static readonly IReadOnlyList<CountryDialCode> All = new List<CountryDialCode>
        {
            new("+92",  "Pakistan"),
            new("+91",  "India"),
            new("+880", "Bangladesh"),
            new("+93",  "Afghanistan"),
            new("+94",  "Sri Lanka"),
            new("+971", "United Arab Emirates"),
            new("+966", "Saudi Arabia"),
            new("+968", "Oman"),
            new("+974", "Qatar"),
            new("+973", "Bahrain"),
            new("+965", "Kuwait"),
            new("+90",  "Turkey"),
            new("+44",  "United Kingdom"),
            new("+1",   "United States / Canada"),
            new("+61",  "Australia"),
            new("+64",  "New Zealand"),
            new("+86",  "China"),
            new("+81",  "Japan"),
            new("+82",  "South Korea"),
            new("+60",  "Malaysia"),
            new("+65",  "Singapore"),
            new("+62",  "Indonesia"),
            new("+63",  "Philippines"),
            new("+66",  "Thailand"),
            new("+49",  "Germany"),
            new("+33",  "France"),
            new("+39",  "Italy"),
            new("+34",  "Spain"),
            new("+31",  "Netherlands"),
            new("+46",  "Sweden"),
            new("+47",  "Norway"),
            new("+45",  "Denmark"),
            new("+353", "Ireland"),
            new("+41",  "Switzerland"),
            new("+7",   "Russia / Kazakhstan"),
            new("+20",  "Egypt"),
            new("+27",  "South Africa"),
            new("+234", "Nigeria"),
            new("+254", "Kenya"),
            new("+98",  "Iran"),
            new("+964", "Iraq"),
            new("+962", "Jordan"),
            new("+961", "Lebanon"),
            new("+55",  "Brazil"),
            new("+52",  "Mexico")
        };
    }

    // ---------------- Employees ----------------

    public class PayrollEmployeeFormViewModel
    {
        public int Id { get; set; }

        public string? EmployeeCode { get; set; }

        [Required(ErrorMessage = "Employee name is required."), MaxLength(150)]
        [Display(Name = "Employee Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Designation"), MaxLength(100)]
        public string? Designation { get; set; }

        [Display(Name = "CNIC Number")]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC must be in the format 00000-0000000-0.")]
        public string? CNICNumber { get; set; }

        [Display(Name = "Blood Group"), MaxLength(3)]
        public string? BloodGroup { get; set; }

        [Display(Name = "Country Code")]
        public string? ContactCountryCode { get; set; } = "+92";

        [Display(Name = "Contact Number")]
        [RegularExpression(@"^[\d\s\-]{6,20}$", ErrorMessage = "Contact number may only contain digits, spaces and dashes.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Enter a valid email address."), MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [Display(Name = "Zip / Postal Code"), MaxLength(20)]
        public string? ZipCode { get; set; }

        /// <summary>Uploaded via the shared _AvatarUpload control.</summary>
        [Display(Name = "Photo")]
        public IFormFile? ProfileImage { get; set; }

        public string? ExistingProfileImagePath { get; set; }

        [Display(Name = "Shift")]
        public int? ShiftTimingId { get; set; }

        [Display(Name = "Basic Salary")]
        [Range(0, 999999999, ErrorMessage = "Basic salary cannot be negative.")]
        public decimal BasicSalary { get; set; }

        [Display(Name = "House Rent Allowance")]
        [Range(0, 999999999)]
        public decimal HouseRentAllowance { get; set; }

        [Display(Name = "Medical Allowance")]
        [Range(0, 999999999)]
        public decimal MedicalAllowance { get; set; }

        [Display(Name = "Other Allowances")]
        [Range(0, 999999999)]
        public decimal OtherAllowances { get; set; }

        [Display(Name = "Bank Name"), MaxLength(150)]
        public string? BankName { get; set; }

        [Display(Name = "Account / IBAN Number"), MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        [Display(Name = "Tax (NTN) Number"), MaxLength(20)]
        public string? TaxNumber { get; set; }

        [Display(Name = "EOBI Number"), MaxLength(30)]
        public string? EOBINumber { get; set; }

        [Display(Name = "Joining Date")]
        public DateTime? JoiningDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<DepartmentOption> Departments { get; set; } = new();
        public List<ShiftOption> Shifts { get; set; } = new();
        public IReadOnlyList<CountryDialCode> DialCodes { get; set; } = Payroll.DialCodes.All;

        /// <summary>BasicSalary + all allowances — shown read-only on the form and stored on Employee.MonthlySalary.</summary>
        public decimal GrossSalary => BasicSalary + HouseRentAllowance + MedicalAllowance + OtherAllowances;
    }

    public class PayrollEmployeeListItemViewModel
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ProfileImagePath { get; set; }
        public string? DepartmentName { get; set; }
        public string? Designation { get; set; }
        public string? ShiftName { get; set; }
        public string? CNICNumber { get; set; }
        public string? Contact { get; set; }
        public decimal BasicSalary { get; set; }
        public bool IsActive { get; set; }
    }

    public class EmployeeDetailViewModel
    {
        public Models.Accounts.Employee Employee { get; set; } = null!;
        public string? DepartmentName { get; set; }
        public string? ShiftName { get; set; }
        public decimal ShiftHours { get; set; }
        public decimal OutstandingAdvance { get; set; }
        public List<SalaryIncrement> Increments { get; set; } = new();
        public List<Payslip> RecentPayslips { get; set; } = new();
    }

    // ---------------- Departments ----------------

    public class DepartmentFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required."), MaxLength(150)]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Code { get; set; }

        [Display(Name = "Parent Department")]
        public int? ParentDepartmentId { get; set; }

        [Display(Name = "Department Head")]
        public int? HeadEmployeeId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<DepartmentOption> ParentOptions { get; set; } = new();
        public List<PayrollEmployeeOption> Employees { get; set; } = new();
    }

    public class DepartmentListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? ParentName { get; set; }
        public string? HeadName { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>One node of the department tree, rendered recursively by the Hierarchy view.</summary>
    public class DepartmentNodeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? HeadName { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
        public List<DepartmentNodeViewModel> Children { get; set; } = new();
    }

    // ---------------- Shift timings ----------------

    public class ShiftTimingFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Shift name is required."), MaxLength(100)]
        [Display(Name = "Shift Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Time")]
        public string StartTime { get; set; } = "09:00";

        [Required]
        [Display(Name = "End Time")]
        public string EndTime { get; set; } = "17:00";

        [Display(Name = "Unpaid Break (minutes)")]
        [Range(0, 360)]
        public int BreakMinutes { get; set; } = 60;

        [Display(Name = "Grace Period (minutes)")]
        [Range(0, 240, ErrorMessage = "Grace period must be between 0 and 240 minutes.")]
        public int GracePeriodMinutes { get; set; } = 15;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class ShiftTimingListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public decimal ShiftHours { get; set; }
        public int BreakMinutes { get; set; }
        public int GracePeriodMinutes { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
    }

    // ---------------- Attendance ----------------

    /// <summary>One employee row on the "Enter Attendance" grid for a single date.</summary>
    public class AttendanceEntryRowViewModel
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? ShiftName { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        /// <summary>HH:mm strings so they bind cleanly to &lt;input type="time"&gt;.</summary>
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }

        public decimal WorkedHours { get; set; }
        public int BreakMinutes { get; set; }

        public string? Remarks { get; set; }
    }

    public class AttendanceEntryViewModel
    {
        [Display(Name = "Attendance Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Through")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        public List<AttendanceEntryRowViewModel> Rows { get; set; } = new();
        public List<DepartmentOption> Departments { get; set; } = new();

        /// <summary>Set when the chosen date is a configured holiday, so the view can warn before saving.</summary>
        public string? HolidayName { get; set; }
    }

    public class AttendanceSheetCellViewModel
    {
        public int Day { get; set; }
        public string Code { get; set; } = "-";
        public string CssClass { get; set; } = "att-none";
        public string Tooltip { get; set; } = string.Empty;
    }

    public class AttendanceSheetRowViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public List<AttendanceSheetCellViewModel> Cells { get; set; } = new();

        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HalfDays { get; set; }
        public int LateCount { get; set; }
        public decimal LateHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal AttendanceHours { get; set; }
    }

    public class AttendanceSheetViewModel
    {
        public int Month { get; set; } = DateTime.Today.Month;
        public int Year { get; set; } = DateTime.Today.Year;
        public int? DepartmentId { get; set; }

        public int DaysInMonth { get; set; }
        public List<AttendanceSheetRowViewModel> Rows { get; set; } = new();
        public List<DepartmentOption> Departments { get; set; } = new();
        public Dictionary<int, string> Holidays { get; set; } = new();

        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }
}
