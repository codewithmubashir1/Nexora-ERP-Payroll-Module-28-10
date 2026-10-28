namespace ERP.Identity.Models
{
    public static class EmploymentOptions
    {
        public static IReadOnlyList<string> BloodGroups { get; } = new[]
        {
            "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"
        };

        public static IReadOnlyList<string> Designations { get; } = new[]
        {
            "Chief Executive Officer", "General Manager", "Manager", "Assistant Manager",
            "Supervisor", "Officer", "Assistant", "Accountant", "HR Officer", "Engineer",
            "Technician", "Operator", "Driver", "Security Guard", "Storekeeper", "Clerk", "Worker", "Other"
        };

        public static IReadOnlyList<string> PakistaniBanks { get; } = new[]
        {
            "Allied Bank Limited", "Askari Bank Limited", "Bank Alfalah Limited", "Bank of Khyber",
            "Bank of Punjab", "BankIslami Pakistan Limited", "Dubai Islamic Bank Pakistan Limited",
            "Faysal Bank Limited", "First Women Bank Limited", "Habib Bank Limited",
            "Habib Metropolitan Bank Limited", "JS Bank Limited", "MCB Bank Limited",
            "Meezan Bank Limited", "National Bank of Pakistan", "Samba Bank Limited",
            "Silkbank Limited", "Sindh Bank Limited", "Soneri Bank Limited",
            "Standard Chartered Bank (Pakistan) Limited", "Summit Bank Limited",
            "The Bank of Azad Jammu & Kashmir", "United Bank Limited",
            "Zarai Taraqiati Bank Limited", "Other"
        };
    }
}
