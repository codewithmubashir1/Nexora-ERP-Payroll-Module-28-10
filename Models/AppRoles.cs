namespace ERP.Identity.Models
{
    /// <summary>
    /// Central place that defines every role available in the ERP system.
    /// Add/remove roles here and the seeder + role dropdowns pick it up automatically.
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Supervisor = "Supervisor";
        public const string Manager = "Manager";
        public const string StandardUser = "Standard User";
        public const string Accountant = "Accountant";
        public const string StoreIncharge = "Store Incharge";

        public static readonly string[] All =
        {
            Admin, Supervisor, Manager, StandardUser, Accountant, StoreIncharge
        };
    }
}
