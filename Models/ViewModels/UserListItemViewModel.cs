namespace ERP.Identity.Models.ViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? ProfileImagePath { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
