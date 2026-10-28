using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Identity.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    AccountType = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSystemAccount = table.Column<bool>(type: "INTEGER", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Accounts_ParentAccountId",
                        column: x => x.ParentAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    EmployeeCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Designation = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NationalId = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "TEXT", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "TEXT", nullable: true),
                    Bio = table.Column<string>(type: "TEXT", nullable: true),
                    ProfileImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    DeviceType = table.Column<int>(type: "INTEGER", nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSyncOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendancePolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicyType = table.Column<int>(type: "INTEGER", nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxOccurrencesPerMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    PenaltyOnExceeding = table.Column<int>(type: "INTEGER", nullable: false),
                    CompensationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OvertimeRateMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendancePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    NTNNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    SalesTaxRegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LogoPath = table.Column<string>(type: "TEXT", nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    BankAccountTitle = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    IsOptional = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntryNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalVouchers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DaysPerYear = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPaid = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEncashable = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineIntegrationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncMode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SyncIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineIntegrationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtherPayrollSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkingDaysPerWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    FiscalYearStartMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    OvertimeRateMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RoundOffNetPay = table.Column<bool>(type: "INTEGER", nullable: false),
                    PayrollCutoffDay = table.Column<int>(type: "INTEGER", nullable: false),
                    MonthlyFixedDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherPayrollSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalEmployees = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReimbursementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaxLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReimbursementTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftTimings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftTimings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatutoryDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RatePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MaxApplicableSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutoryDeductions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxSlabs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MinIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RatePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxSlabs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankPaymentVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BankAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContraAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidTo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    PartyType = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClearanceStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankPaymentVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankPaymentVouchers_Accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankPaymentVouchers_Accounts_ContraAccountId",
                        column: x => x.ContraAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankReceiptVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BankAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContraAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceivedFrom = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    PartyType = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClearanceStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankReceiptVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankReceiptVouchers_Accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankReceiptVouchers_Accounts_ContraAccountId",
                        column: x => x.ContraAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashPaymentVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CashAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContraAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidTo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    PartyType = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashPaymentVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashPaymentVouchers_Accounts_CashAccountId",
                        column: x => x.CashAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashPaymentVouchers_Accounts_ContraAccountId",
                        column: x => x.ContraAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashReceiptVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CashAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContraAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceivedFrom = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    PartyType = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashReceiptVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashReceiptVouchers_Accounts_CashAccountId",
                        column: x => x.CashAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashReceiptVouchers_Accounts_ContraAccountId",
                        column: x => x.ContraAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerCode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CustomerCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NTNNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SalesTaxRegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    OurVendorCodeAtCustomer = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PaymentTermsDays = table.Column<int>(type: "INTEGER", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_CustomerCategories_CustomerCategoryId",
                        column: x => x.CustomerCategoryId,
                        principalTable: "CustomerCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JournalEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PartyType = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalVoucherLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JournalVoucherId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PartyType = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<int>(type: "INTEGER", nullable: true),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalVoucherLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalVoucherLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalVoucherLines_JournalVouchers_JournalVoucherId",
                        column: x => x.JournalVoucherId,
                        principalTable: "JournalVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsumptionVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoucherNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    IssuedTo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumptionVouchers_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransferNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FromLocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToLocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalTransfers_Locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalTransfers_Locations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemAdjustmentNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdjustmentNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAdjustmentNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemAdjustmentNotes_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Sku = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ProductCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HsCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitId = table.Column<int>(type: "INTEGER", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ReorderLevel = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OpeningStock = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DefaultLocationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ProductImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    ProductType = table.Column<int>(type: "INTEGER", nullable: false),
                    PackingUnitName = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    PackingQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Locations_DefaultLocationId",
                        column: x => x.DefaultLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_UnitsOfMeasure_UnitId",
                        column: x => x.UnitId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VendorCode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    VendorCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NTNNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    SalesTaxRegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PaymentTermsDays = table.Column<int>(type: "INTEGER", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendors_VendorCategories_VendorCategoryId",
                        column: x => x.VendorCategoryId,
                        principalTable: "VendorCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaleOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SONumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    SODate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CustomerPONumbers = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VoucherType = table.Column<int>(type: "INTEGER", nullable: false),
                    InventoryAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContraAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemAdjustmentNoteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Narration = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryVouchers_Accounts_ContraAccountId",
                        column: x => x.ContraAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryVouchers_Accounts_InventoryAccountId",
                        column: x => x.InventoryAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryVouchers_ItemAdjustmentNotes_ItemAdjustmentNoteId",
                        column: x => x.ItemAdjustmentNoteId,
                        principalTable: "ItemAdjustmentNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsumptionVoucherItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsumptionVoucherId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionVoucherItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumptionVoucherItems_ConsumptionVouchers_ConsumptionVoucherId",
                        column: x => x.ConsumptionVoucherId,
                        principalTable: "ConsumptionVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsumptionVoucherItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerProductAliases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CustomerProductCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerProductAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerProductAliases_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalTransferItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InternalTransferId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalTransferItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalTransferItems_InternalTransfers_InternalTransferId",
                        column: x => x.InternalTransferId,
                        principalTable: "InternalTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternalTransferItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemAdjustmentNoteItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemAdjustmentNoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    AdjustmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAdjustmentNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemAdjustmentNoteItems_ItemAdjustmentNotes_ItemAdjustmentNoteId",
                        column: x => x.ItemAdjustmentNoteId,
                        principalTable: "ItemAdjustmentNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemAdjustmentNoteItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductPriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    OldCostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewCostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldSalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewSalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChangedById = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPriceHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TransactionType = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ReferenceType = table.Column<string>(type: "TEXT", nullable: false),
                    ReferenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceNo = table.Column<string>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssuedCheques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChequeReferenceNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ChequeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ClearanceStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuedCheques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssuedCheques_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PONumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PODate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryChallans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChallanNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ChallanDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SaleOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    VehicleNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    DriverName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryChallans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryChallans_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryChallans_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryChallans_SaleOrders_SaleOrderId",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaleOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaleOrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleOrderItems_SaleOrders_SaleOrderId",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorAdvances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdvanceNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    AdvanceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    IssuedChequeId = table.Column<int>(type: "INTEGER", nullable: true),
                    AdjustedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorAdvances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorAdvances_IssuedCheques_IssuedChequeId",
                        column: x => x.IssuedChequeId,
                        principalTable: "IssuedCheques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorAdvances_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InwardGatePasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GatePassNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    GatePassDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    VehicleNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    DriverName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InwardGatePasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InwardGatePasses_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InwardGatePasses_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PurchaseOrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryChallanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeliveryChallanId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryChallanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanItems_DeliveryChallans_DeliveryChallanId",
                        column: x => x.DeliveryChallanId,
                        principalTable: "DeliveryChallans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PaymentTermsDays = table.Column<int>(type: "INTEGER", nullable: true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SaleOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeliveryChallanId = table.Column<int>(type: "INTEGER", nullable: true),
                    CustomerPONumbers = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesInvoices_DeliveryChallans_DeliveryChallanId",
                        column: x => x.DeliveryChallanId,
                        principalTable: "DeliveryChallans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesInvoices_SaleOrders_SaleOrderId",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GRNNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    GRNDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    InwardGatePassId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNotes_InwardGatePasses_InwardGatePassId",
                        column: x => x.InwardGatePassId,
                        principalTable: "InwardGatePasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNotes_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNotes_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNotes_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InwardGatePassItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InwardGatePassId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeclaredQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InwardGatePassItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InwardGatePassItems_InwardGatePasses_InwardGatePassId",
                        column: x => x.InwardGatePassId,
                        principalTable: "InwardGatePasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InwardGatePassItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditNoteNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CreditNoteDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SalesInvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeliveryChallanId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditNotes_DeliveryChallans_DeliveryChallanId",
                        column: x => x.DeliveryChallanId,
                        principalTable: "DeliveryChallans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditNotes_SalesInvoices_SalesInvoiceId",
                        column: x => x.SalesInvoiceId,
                        principalTable: "SalesInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalesInvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceItems_SalesInvoices_SalesInvoiceId",
                        column: x => x.SalesInvoiceId,
                        principalTable: "SalesInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoicePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PaymentNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SalesInvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WithholdingTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    WithholdingTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    ClearanceStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoicePayments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesInvoicePayments_SalesInvoices_SalesInvoiceId",
                        column: x => x.SalesInvoiceId,
                        principalTable: "SalesInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptNoteItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GoodsReceiptNoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeclaredQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    RejectedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNoteItems_GoodsReceiptNotes_GoodsReceiptNoteId",
                        column: x => x.GoodsReceiptNoteId,
                        principalTable: "GoodsReceiptNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNoteItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    VendorInvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    GoodsReceiptNoteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WithholdingTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    WithholdingTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPayableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoices_GoodsReceiptNotes_GoodsReceiptNoteId",
                        column: x => x.GoodsReceiptNoteId,
                        principalTable: "GoodsReceiptNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoices_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoices_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreditNoteItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditNoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNoteItems_CreditNotes_CreditNoteId",
                        column: x => x.CreditNoteId,
                        principalTable: "CreditNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditNoteItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DebitNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DebitNoteNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DebitNoteDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchaseInvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    GoodsReceiptNoteId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitNotes_GoodsReceiptNotes_GoodsReceiptNoteId",
                        column: x => x.GoodsReceiptNoteId,
                        principalTable: "GoodsReceiptNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DebitNotes_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DebitNotes_PurchaseInvoices_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DebitNotes_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PurchaseInvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceItems_PurchaseInvoices_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoicePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PaymentNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchaseInvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WithholdingTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    WithholdingTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    IssuedChequeId = table.Column<int>(type: "INTEGER", nullable: true),
                    VendorAdvanceId = table.Column<int>(type: "INTEGER", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoicePayments_IssuedCheques_IssuedChequeId",
                        column: x => x.IssuedChequeId,
                        principalTable: "IssuedCheques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoicePayments_PurchaseInvoices_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoicePayments_VendorAdvances_VendorAdvanceId",
                        column: x => x.VendorAdvanceId,
                        principalTable: "VendorAdvances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoicePayments_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DebitNoteItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DebitNoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitNoteItems_DebitNotes_DebitNoteId",
                        column: x => x.DebitNoteId,
                        principalTable: "DebitNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DebitNoteItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckInTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    IsLate = table.Column<bool>(type: "INTEGER", nullable: false),
                    LateByMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEarlyDeparture = table.Column<bool>(type: "INTEGER", nullable: false),
                    EarlyByMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    ParentDepartmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    HeadEmployeeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeCode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ShiftTimingId = table.Column<int>(type: "INTEGER", nullable: true),
                    Designation = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CNICNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    ContactCountryCode = table.Column<string>(type: "TEXT", maxLength: 6, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ProfileImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HouseRentAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MedicalAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TaxNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EOBINumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_ShiftTimings_ShiftTimingId",
                        column: x => x.ShiftTimingId,
                        principalTable: "ShiftTimings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveEncashmentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    DaysToEncash = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveEncashmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveEncashmentRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveEncashmentRequests_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ToDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanAdvanceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestType = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstallmentMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanAdvanceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanAdvanceRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MidSalaryRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MidSalaryRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MidSalaryRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payslips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PayslipNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PayRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HouseRentAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MedicalAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WorkingDays = table.Column<int>(type: "INTEGER", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LateHours = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NetHours = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    OvertimeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvancePreviousBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentAdvanceGiven = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAdvance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvanceDeducted = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvanceRemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FixedMonthlyDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatutoryDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AbsentDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PresentDays = table.Column<int>(type: "INTEGER", nullable: false),
                    AbsentDays = table.Column<int>(type: "INTEGER", nullable: false),
                    LeaveDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payslips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payslips_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payslips_PayRuns_PayRunId",
                        column: x => x.PayRunId,
                        principalTable: "PayRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReimbursementRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReimbursementTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ReceiptImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReimbursementRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReimbursementRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReimbursementRequests_ReimbursementTypes_ReimbursementTypeId",
                        column: x => x.ReimbursementTypeId,
                        principalTable: "ReimbursementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryIncrements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OldBasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewBasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedByName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryIncrements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryIncrements_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountCode",
                table: "Accounts",
                column: "AccountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ParentAccountId",
                table: "Accounts",
                column: "ParentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeId_Date",
                table: "AttendanceRecords",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankPaymentVouchers_BankAccountId",
                table: "BankPaymentVouchers",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankPaymentVouchers_ContraAccountId",
                table: "BankPaymentVouchers",
                column: "ContraAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankPaymentVouchers_VoucherNumber",
                table: "BankPaymentVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankReceiptVouchers_BankAccountId",
                table: "BankReceiptVouchers",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankReceiptVouchers_ContraAccountId",
                table: "BankReceiptVouchers",
                column: "ContraAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankReceiptVouchers_VoucherNumber",
                table: "BankReceiptVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashPaymentVouchers_CashAccountId",
                table: "CashPaymentVouchers",
                column: "CashAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashPaymentVouchers_ContraAccountId",
                table: "CashPaymentVouchers",
                column: "ContraAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashPaymentVouchers_VoucherNumber",
                table: "CashPaymentVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashReceiptVouchers_CashAccountId",
                table: "CashReceiptVouchers",
                column: "CashAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceiptVouchers_ContraAccountId",
                table: "CashReceiptVouchers",
                column: "ContraAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashReceiptVouchers_VoucherNumber",
                table: "CashReceiptVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionVoucherItems_ConsumptionVoucherId",
                table: "ConsumptionVoucherItems",
                column: "ConsumptionVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionVoucherItems_ProductId",
                table: "ConsumptionVoucherItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionVouchers_LocationId",
                table: "ConsumptionVouchers",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionVouchers_VoucherNo",
                table: "ConsumptionVouchers",
                column: "VoucherNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteItems_CreditNoteId",
                table: "CreditNoteItems",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteItems_ProductId",
                table: "CreditNoteItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_CreditNoteNumber",
                table: "CreditNotes",
                column: "CreditNoteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_CustomerId",
                table: "CreditNotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_DeliveryChallanId",
                table: "CreditNotes",
                column: "DeliveryChallanId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_LocationId",
                table: "CreditNotes",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_SalesInvoiceId",
                table: "CreditNotes",
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProductAliases_CustomerName_ProductId",
                table: "CustomerProductAliases",
                columns: new[] { "CustomerName", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProductAliases_ProductId",
                table: "CustomerProductAliases",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCategoryId",
                table: "Customers",
                column: "CustomerCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers",
                column: "CustomerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteItems_DebitNoteId",
                table: "DebitNoteItems",
                column: "DebitNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteItems_ProductId",
                table: "DebitNoteItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_DebitNoteNumber",
                table: "DebitNotes",
                column: "DebitNoteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_GoodsReceiptNoteId",
                table: "DebitNotes",
                column: "GoodsReceiptNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_LocationId",
                table: "DebitNotes",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_PurchaseInvoiceId",
                table: "DebitNotes",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_VendorId",
                table: "DebitNotes",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_DeliveryChallanId",
                table: "DeliveryChallanItems",
                column: "DeliveryChallanId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_ProductId",
                table: "DeliveryChallanItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_ChallanNo",
                table: "DeliveryChallans",
                column: "ChallanNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_CustomerId",
                table: "DeliveryChallans",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_LocationId",
                table: "DeliveryChallans",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_SaleOrderId",
                table: "DeliveryChallans",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadEmployeeId",
                table: "Departments",
                column: "HeadEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ShiftTimingId",
                table: "Employees",
                column: "ShiftTimingId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNoteItems_GoodsReceiptNoteId",
                table: "GoodsReceiptNoteItems",
                column: "GoodsReceiptNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNoteItems_ProductId",
                table: "GoodsReceiptNoteItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_GRNNumber",
                table: "GoodsReceiptNotes",
                column: "GRNNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_InwardGatePassId",
                table: "GoodsReceiptNotes",
                column: "InwardGatePassId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_LocationId",
                table: "GoodsReceiptNotes",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_PurchaseOrderId",
                table: "GoodsReceiptNotes",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_VendorId",
                table: "GoodsReceiptNotes",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransferItems_InternalTransferId",
                table: "InternalTransferItems",
                column: "InternalTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransferItems_ProductId",
                table: "InternalTransferItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransfers_FromLocationId",
                table: "InternalTransfers",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransfers_ToLocationId",
                table: "InternalTransfers",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransfers_TransferNo",
                table: "InternalTransfers",
                column: "TransferNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryVouchers_ContraAccountId",
                table: "InventoryVouchers",
                column: "ContraAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryVouchers_InventoryAccountId",
                table: "InventoryVouchers",
                column: "InventoryAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryVouchers_ItemAdjustmentNoteId",
                table: "InventoryVouchers",
                column: "ItemAdjustmentNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryVouchers_VoucherNumber",
                table: "InventoryVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InwardGatePasses_GatePassNo",
                table: "InwardGatePasses",
                column: "GatePassNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InwardGatePasses_PurchaseOrderId",
                table: "InwardGatePasses",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InwardGatePasses_VendorId",
                table: "InwardGatePasses",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_InwardGatePassItems_InwardGatePassId",
                table: "InwardGatePassItems",
                column: "InwardGatePassId");

            migrationBuilder.CreateIndex(
                name: "IX_InwardGatePassItems_ProductId",
                table: "InwardGatePassItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedCheques_ChequeReferenceNo",
                table: "IssuedCheques",
                column: "ChequeReferenceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssuedCheques_VendorId",
                table: "IssuedCheques",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAdjustmentNoteItems_ItemAdjustmentNoteId",
                table: "ItemAdjustmentNoteItems",
                column: "ItemAdjustmentNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAdjustmentNoteItems_ProductId",
                table: "ItemAdjustmentNoteItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAdjustmentNotes_AdjustmentNo",
                table: "ItemAdjustmentNotes",
                column: "AdjustmentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemAdjustmentNotes_LocationId",
                table: "ItemAdjustmentNotes",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryNumber",
                table: "JournalEntries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountId",
                table: "JournalEntryLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVoucherLines_AccountId",
                table: "JournalVoucherLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVoucherLines_JournalVoucherId",
                table: "JournalVoucherLines",
                column: "JournalVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_VoucherNumber",
                table: "JournalVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEncashmentRequests_EmployeeId",
                table: "LeaveEncashmentRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEncashmentRequests_LeaveTypeId",
                table: "LeaveEncashmentRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEncashmentRequests_RequestNumber",
                table: "LeaveEncashmentRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_RequestNumber",
                table: "LeaveRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanAdvanceRequests_EmployeeId",
                table: "LoanAdvanceRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAdvanceRequests_RequestNumber",
                table: "LoanAdvanceRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MidSalaryRequests_EmployeeId",
                table: "MidSalaryRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MidSalaryRequests_RequestNumber",
                table: "MidSalaryRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayRuns_RunNumber",
                table: "PayRuns",
                column: "RunNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_EmployeeId",
                table: "Payslips",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_PayRunId",
                table: "Payslips",
                column: "PayRunId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_PayslipNumber",
                table: "Payslips",
                column: "PayslipNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ProductId",
                table: "ProductPriceHistories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultLocationId",
                table: "Products",
                column: "DefaultLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitId",
                table: "Products",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItems_ProductId",
                table: "PurchaseInvoiceItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItems_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoicePayments_IssuedChequeId",
                table: "PurchaseInvoicePayments",
                column: "IssuedChequeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoicePayments_PaymentNumber",
                table: "PurchaseInvoicePayments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoicePayments_PurchaseInvoiceId",
                table: "PurchaseInvoicePayments",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoicePayments_VendorAdvanceId",
                table: "PurchaseInvoicePayments",
                column: "VendorAdvanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoicePayments_VendorId",
                table: "PurchaseInvoicePayments",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_GoodsReceiptNoteId",
                table: "PurchaseInvoices",
                column: "GoodsReceiptNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_InvoiceNumber",
                table: "PurchaseInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_PurchaseOrderId",
                table: "PurchaseInvoices",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_VendorId",
                table: "PurchaseInvoices",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PONumber",
                table: "PurchaseOrders",
                column: "PONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_VendorId",
                table: "PurchaseOrders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_EmployeeId",
                table: "ReimbursementRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_ReimbursementTypeId",
                table: "ReimbursementRequests",
                column: "ReimbursementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReimbursementRequests_RequestNumber",
                table: "ReimbursementRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryIncrements_EmployeeId",
                table: "SalaryIncrements",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderItems_ProductId",
                table: "SaleOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderItems_SaleOrderId",
                table: "SaleOrderItems",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_CustomerId",
                table: "SaleOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_SONumber",
                table: "SaleOrders",
                column: "SONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItems_ProductId",
                table: "SalesInvoiceItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItems_SalesInvoiceId",
                table: "SalesInvoiceItems",
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoicePayments_CustomerId",
                table: "SalesInvoicePayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoicePayments_PaymentNumber",
                table: "SalesInvoicePayments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoicePayments_SalesInvoiceId",
                table: "SalesInvoicePayments",
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_CustomerId",
                table: "SalesInvoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_DeliveryChallanId",
                table: "SalesInvoices",
                column: "DeliveryChallanId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_InvoiceNumber",
                table: "SalesInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_SaleOrderId",
                table: "SalesInvoices",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_LocationId",
                table: "StockTransactions",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ProductId_LocationId",
                table: "StockTransactions",
                columns: new[] { "ProductId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendorAdvances_AdvanceNumber",
                table: "VendorAdvances",
                column: "AdvanceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorAdvances_IssuedChequeId",
                table: "VendorAdvances",
                column: "IssuedChequeId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorAdvances_VendorId",
                table: "VendorAdvances",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_VendorCategoryId",
                table: "Vendors",
                column: "VendorCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_VendorCode",
                table: "Vendors",
                column: "VendorCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Employees_EmployeeId",
                table: "AttendanceRecords",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_HeadEmployeeId",
                table: "Departments",
                column: "HeadEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_HeadEmployeeId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AttendanceDevices");

            migrationBuilder.DropTable(
                name: "AttendancePolicies");

            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "BankPaymentVouchers");

            migrationBuilder.DropTable(
                name: "BankReceiptVouchers");

            migrationBuilder.DropTable(
                name: "CashPaymentVouchers");

            migrationBuilder.DropTable(
                name: "CashReceiptVouchers");

            migrationBuilder.DropTable(
                name: "CompanySettings");

            migrationBuilder.DropTable(
                name: "ConsumptionVoucherItems");

            migrationBuilder.DropTable(
                name: "CreditNoteItems");

            migrationBuilder.DropTable(
                name: "CustomerProductAliases");

            migrationBuilder.DropTable(
                name: "DebitNoteItems");

            migrationBuilder.DropTable(
                name: "DeliveryChallanItems");

            migrationBuilder.DropTable(
                name: "GoodsReceiptNoteItems");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "InternalTransferItems");

            migrationBuilder.DropTable(
                name: "InventoryVouchers");

            migrationBuilder.DropTable(
                name: "InwardGatePassItems");

            migrationBuilder.DropTable(
                name: "ItemAdjustmentNoteItems");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "JournalVoucherLines");

            migrationBuilder.DropTable(
                name: "LeaveEncashmentRequests");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "LoanAdvanceRequests");

            migrationBuilder.DropTable(
                name: "MachineIntegrationSettings");

            migrationBuilder.DropTable(
                name: "MidSalaryRequests");

            migrationBuilder.DropTable(
                name: "OtherPayrollSettings");

            migrationBuilder.DropTable(
                name: "Payslips");

            migrationBuilder.DropTable(
                name: "ProductPriceHistories");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceItems");

            migrationBuilder.DropTable(
                name: "PurchaseInvoicePayments");

            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "ReimbursementRequests");

            migrationBuilder.DropTable(
                name: "SalaryIncrements");

            migrationBuilder.DropTable(
                name: "SaleOrderItems");

            migrationBuilder.DropTable(
                name: "SalesInvoiceItems");

            migrationBuilder.DropTable(
                name: "SalesInvoicePayments");

            migrationBuilder.DropTable(
                name: "StatutoryDeductions");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "TaxSlabs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ConsumptionVouchers");

            migrationBuilder.DropTable(
                name: "CreditNotes");

            migrationBuilder.DropTable(
                name: "DebitNotes");

            migrationBuilder.DropTable(
                name: "InternalTransfers");

            migrationBuilder.DropTable(
                name: "ItemAdjustmentNotes");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "JournalVouchers");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "PayRuns");

            migrationBuilder.DropTable(
                name: "VendorAdvances");

            migrationBuilder.DropTable(
                name: "ReimbursementTypes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "SalesInvoices");

            migrationBuilder.DropTable(
                name: "PurchaseInvoices");

            migrationBuilder.DropTable(
                name: "IssuedCheques");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure");

            migrationBuilder.DropTable(
                name: "DeliveryChallans");

            migrationBuilder.DropTable(
                name: "GoodsReceiptNotes");

            migrationBuilder.DropTable(
                name: "SaleOrders");

            migrationBuilder.DropTable(
                name: "InwardGatePasses");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "CustomerCategories");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "VendorCategories");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "ShiftTimings");
        }
    }
}
