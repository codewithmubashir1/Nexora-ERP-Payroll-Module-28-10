using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.Payroll
{
    public class Department
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Code { get; set; }

        public int? ParentDepartmentId { get; set; }
        public Department? ParentDepartment { get; set; }

        public int? HeadEmployeeId { get; set; }
        public Accounts.Employee? HeadEmployee { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
    }
}
