CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Accounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Accounts" PRIMARY KEY AUTOINCREMENT,
    "AccountCode" TEXT NOT NULL,
    "AccountName" TEXT NOT NULL,
    "AccountType" INTEGER NOT NULL,
    "ParentAccountId" INTEGER NULL,
    "IsSystemAccount" INTEGER NOT NULL,
    "OpeningBalance" decimal(18,2) NOT NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_Accounts_Accounts_ParentAccountId" FOREIGN KEY ("ParentAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL
);

CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "FullName" TEXT NOT NULL,
    "EmployeeCode" TEXT NULL,
    "Designation" TEXT NULL,
    "Department" TEXT NULL,
    "Gender" TEXT NULL,
    "DateOfBirth" TEXT NULL,
    "JoiningDate" TEXT NULL,
    "NationalId" TEXT NULL,
    "Address" TEXT NULL,
    "City" TEXT NULL,
    "Country" TEXT NULL,
    "EmergencyContactName" TEXT NULL,
    "EmergencyContactPhone" TEXT NULL,
    "Bio" TEXT NULL,
    "ProfileImagePath" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    "LastLoginOn" TEXT NULL,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL
);

CREATE TABLE "AttendanceDevices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AttendanceDevices" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Location" TEXT NULL,
    "DeviceType" INTEGER NOT NULL,
    "IPAddress" TEXT NULL,
    "SerialNumber" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "LastSyncOn" TEXT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "AttendancePolicies" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AttendancePolicies" PRIMARY KEY AUTOINCREMENT,
    "PolicyType" INTEGER NOT NULL,
    "GracePeriodMinutes" INTEGER NOT NULL,
    "MaxOccurrencesPerMonth" INTEGER NOT NULL,
    "PenaltyOnExceeding" INTEGER NOT NULL,
    "CompensationType" TEXT NULL,
    "OvertimeRateMultiplier" decimal(5,2) NULL,
    "Notes" TEXT NULL,
    "UpdatedOn" TEXT NOT NULL
);

CREATE TABLE "CompanySettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CompanySettings" PRIMARY KEY AUTOINCREMENT,
    "CompanyName" TEXT NOT NULL,
    "Address" TEXT NULL,
    "Email" TEXT NULL,
    "Phone" TEXT NULL,
    "NTNNumber" TEXT NULL,
    "SalesTaxRegistrationNumber" TEXT NULL,
    "LogoPath" TEXT NULL,
    "BankName" TEXT NULL,
    "BankAccountTitle" TEXT NULL,
    "BankAccountNumber" TEXT NULL,
    "UpdatedOn" TEXT NOT NULL
);

CREATE TABLE "CustomerCategories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CustomerCategories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Code" TEXT NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "Holidays" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Holidays" PRIMARY KEY AUTOINCREMENT,
    "Date" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "IsOptional" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "JournalEntries" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JournalEntries" PRIMARY KEY AUTOINCREMENT,
    "EntryNumber" TEXT NOT NULL,
    "EntryDate" TEXT NOT NULL,
    "SourceType" TEXT NOT NULL,
    "SourceId" INTEGER NOT NULL,
    "Narration" TEXT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "JournalVouchers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JournalVouchers" PRIMARY KEY AUTOINCREMENT,
    "VoucherNumber" TEXT NOT NULL,
    "VoucherDate" TEXT NOT NULL,
    "Narration" TEXT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "LeaveTypes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_LeaveTypes" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "DaysPerYear" INTEGER NOT NULL,
    "IsPaid" INTEGER NOT NULL,
    "IsEncashable" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "Locations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Locations" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Code" TEXT NULL,
    "Address" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "MachineIntegrationSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachineIntegrationSettings" PRIMARY KEY AUTOINCREMENT,
    "SyncMode" TEXT NOT NULL,
    "SyncIntervalMinutes" INTEGER NOT NULL,
    "ApiEndpoint" TEXT NULL,
    "ApiKey" TEXT NULL,
    "Notes" TEXT NULL,
    "UpdatedOn" TEXT NOT NULL
);

CREATE TABLE "OtherPayrollSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OtherPayrollSettings" PRIMARY KEY AUTOINCREMENT,
    "WorkingDaysPerWeek" INTEGER NOT NULL,
    "FiscalYearStartMonth" INTEGER NOT NULL,
    "DefaultCurrency" TEXT NOT NULL,
    "OvertimeRateMultiplier" decimal(5,2) NOT NULL,
    "RoundOffNetPay" INTEGER NOT NULL,
    "PayrollCutoffDay" INTEGER NOT NULL,
    "MonthlyFixedDeduction" decimal(18,2) NOT NULL,
    "UpdatedOn" TEXT NOT NULL
);

CREATE TABLE "PayRuns" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PayRuns" PRIMARY KEY AUTOINCREMENT,
    "RunNumber" TEXT NOT NULL,
    "Month" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "TotalEmployees" INTEGER NOT NULL,
    "TotalGrossAmount" decimal(18,2) NOT NULL,
    "TotalDeductions" decimal(18,2) NOT NULL,
    "TotalNetAmount" decimal(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "ProductCategories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductCategories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Code" TEXT NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "ReimbursementTypes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ReimbursementTypes" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "MaxLimit" decimal(18,2) NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "ShiftTimings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ShiftTimings" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "StartTime" TEXT NOT NULL,
    "EndTime" TEXT NOT NULL,
    "GracePeriodMinutes" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "StatutoryDeductions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StatutoryDeductions" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "RatePercent" decimal(5,2) NOT NULL,
    "MaxApplicableSalary" decimal(18,2) NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "TaxSlabs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TaxSlabs" PRIMARY KEY AUTOINCREMENT,
    "MinIncome" decimal(18,2) NOT NULL,
    "MaxIncome" decimal(18,2) NULL,
    "RatePercent" decimal(5,2) NOT NULL,
    "FixedAmount" decimal(18,2) NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "UnitsOfMeasure" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_UnitsOfMeasure" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "ShortName" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "VendorCategories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_VendorCategories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Code" TEXT NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL
);

CREATE TABLE "BankPaymentVouchers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BankPaymentVouchers" PRIMARY KEY AUTOINCREMENT,
    "VoucherNumber" TEXT NOT NULL,
    "VoucherDate" TEXT NOT NULL,
    "BankAccountId" INTEGER NOT NULL,
    "ContraAccountId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "PaidTo" TEXT NULL,
    "PartyType" INTEGER NOT NULL,
    "PartyId" INTEGER NULL,
    "PaymentMethod" INTEGER NOT NULL,
    "ChequeNumber" TEXT NULL,
    "ChequeDate" TEXT NULL,
    "ClearanceStatus" INTEGER NULL,
    "Narration" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_BankPaymentVouchers_Accounts_BankAccountId" FOREIGN KEY ("BankAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BankPaymentVouchers_Accounts_ContraAccountId" FOREIGN KEY ("ContraAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "BankReceiptVouchers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BankReceiptVouchers" PRIMARY KEY AUTOINCREMENT,
    "VoucherNumber" TEXT NOT NULL,
    "VoucherDate" TEXT NOT NULL,
    "BankAccountId" INTEGER NOT NULL,
    "ContraAccountId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "ReceivedFrom" TEXT NULL,
    "PartyType" INTEGER NOT NULL,
    "PartyId" INTEGER NULL,
    "PaymentMethod" INTEGER NOT NULL,
    "ChequeNumber" TEXT NULL,
    "ChequeDate" TEXT NULL,
    "ClearanceStatus" INTEGER NULL,
    "Narration" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_BankReceiptVouchers_Accounts_BankAccountId" FOREIGN KEY ("BankAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BankReceiptVouchers_Accounts_ContraAccountId" FOREIGN KEY ("ContraAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CashPaymentVouchers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CashPaymentVouchers" PRIMARY KEY AUTOINCREMENT,
    "VoucherNumber" TEXT NOT NULL,
    "VoucherDate" TEXT NOT NULL,
    "CashAccountId" INTEGER NOT NULL,
    "ContraAccountId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "PaidTo" TEXT NULL,
    "PartyType" INTEGER NOT NULL,
    "PartyId" INTEGER NULL,
    "Narration" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_CashPaymentVouchers_Accounts_CashAccountId" FOREIGN KEY ("CashAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashPaymentVouchers_Accounts_ContraAccountId" FOREIGN KEY ("ContraAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CashReceiptVouchers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CashReceiptVouchers" PRIMARY KEY AUTOINCREMENT,
    "VoucherNumber" TEXT NOT NULL,
    "VoucherDate" TEXT NOT NULL,
    "CashAccountId" INTEGER NOT NULL,
    "ContraAccountId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "ReceivedFrom" TEXT NULL,
    "PartyType" INTEGER NOT NULL,
    "PartyId" INTEGER NULL,
    "Narration" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_CashReceiptVouchers_Accounts_CashAccountId" FOREIGN KEY ("CashAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashReceiptVouchers_Accounts_ContraAccountId" FOREIGN KEY ("ContraAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Customers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Customers" PRIMARY KEY AUTOINCREMENT,
    "CustomerCode" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "CustomerCategoryId" INTEGER NOT NULL,
    "ContactPerson" TEXT NULL,
    "Phone" TEXT NULL,
    "Email" TEXT NULL,
    "Address" TEXT NULL,
    "City" TEXT NULL,
    "Country" TEXT NULL,
    "NTNNumber" TEXT NULL,
    "SalesTaxRegistrationNumber" TEXT NULL,
    "OurVendorCodeAtCustomer" TEXT NULL,
    "PaymentTermsDays" INTEGER NOT NULL,
    "OpeningBalance" decimal(18,2) NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_Customers_CustomerCategories_CustomerCategoryId" FOREIGN KEY ("CustomerCategoryId") REFERENCES "CustomerCategories" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "JournalEntryLines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JournalEntryLines" PRIMARY KEY AUTOINCREMENT,
    "JournalEntryId" INTEGER NOT NULL,
    "AccountId" INTEGER NOT NULL,
    "Debit" decimal(18,2) NOT NULL,
    "Credit" decimal(18,2) NOT NULL,
    "PartyType" INTEGER NOT NULL,
    "PartyId" INTEGER NULL,
    "Narration" TEXT NULL,
    CONSTRAINT "FK_JournalEntryLines_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_JournalEntryLines_JournalEntries_JournalEntryId" FOREIGN KEY ("JournalEntryId") REFERENCES "JournalEntries" ("Id") ON DELETE CASCADE
);

CREATE TABLE "JournalVoucherLines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_JournalVoucherLines" PRIMARY KEY AUTOINCREMENT,
    "JournalVoucherId" INTEGER NOT NULL,
    "AccountId" INTEGER NOT NULL,
    "Debit" decimal(18,2) NOT NULL,
    "Credit" decimal(18,2) NOT NULL,
    "PartyType" INTEGER NOT NULL,
    "PartyId" INTEGER NULL,
    "Narration" TEXT NULL,
    CONSTRAINT "FK_JournalVoucherLines_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_JournalVoucherLines_JournalVouchers_JournalVoucherId" FOREIGN KEY ("JournalVoucherId") REFERENCES "JournalVouchers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ConsumptionVouchers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConsumptionVouchers" PRIMARY KEY AUTOINCREMENT,
    "VoucherNo" TEXT NOT NULL,
    "VoucherDate" TEXT NOT NULL,
    "LocationId" INTEGER NOT NULL,
    "Department" TEXT NULL,
    "IssuedTo" TEXT NULL,
    "Purpose" TEXT NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_ConsumptionVouchers_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "InternalTransfers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InternalTransfers" PRIMARY KEY AUTOINCREMENT,
    "TransferNo" TEXT NOT NULL,
    "TransferDate" TEXT NOT NULL,
    "FromLocationId" INTEGER NOT NULL,
    "ToLocationId" INTEGER NOT NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_InternalTransfers_Locations_FromLocationId" FOREIGN KEY ("FromLocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InternalTransfers_Locations_ToLocationId" FOREIGN KEY ("ToLocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ItemAdjustmentNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ItemAdjustmentNotes" PRIMARY KEY AUTOINCREMENT,
    "AdjustmentNo" TEXT NOT NULL,
    "AdjustmentDate" TEXT NOT NULL,
    "LocationId" INTEGER NOT NULL,
    "Reason" TEXT NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_ItemAdjustmentNotes_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Products" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "Sku" TEXT NOT NULL,
    "ProductCode" TEXT NULL,
    "HsCode" TEXT NULL,
    "Name" TEXT NOT NULL,
    "CategoryId" INTEGER NOT NULL,
    "UnitId" INTEGER NOT NULL,
    "CostPrice" decimal(18,2) NOT NULL,
    "SalePrice" decimal(18,2) NOT NULL,
    "TaxRate" decimal(5,2) NULL,
    "ReorderLevel" decimal(18,3) NOT NULL,
    "OpeningStock" decimal(18,3) NOT NULL,
    "DefaultLocationId" INTEGER NULL,
    "Description" TEXT NULL,
    "ProductImagePath" TEXT NULL,
    "ProductType" INTEGER NOT NULL,
    "PackingUnitName" TEXT NULL,
    "PackingQuantity" decimal(18,3) NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_Products_Locations_DefaultLocationId" FOREIGN KEY ("DefaultLocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Products_ProductCategories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "ProductCategories" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Products_UnitsOfMeasure_UnitId" FOREIGN KEY ("UnitId") REFERENCES "UnitsOfMeasure" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Vendors" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Vendors" PRIMARY KEY AUTOINCREMENT,
    "VendorCode" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "VendorCategoryId" INTEGER NOT NULL,
    "ContactPerson" TEXT NULL,
    "Phone" TEXT NULL,
    "Email" TEXT NULL,
    "Address" TEXT NULL,
    "City" TEXT NULL,
    "Country" TEXT NULL,
    "NTNNumber" TEXT NULL,
    "SalesTaxRegistrationNumber" TEXT NULL,
    "PaymentTermsDays" INTEGER NOT NULL,
    "OpeningBalance" decimal(18,2) NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_Vendors_VendorCategories_VendorCategoryId" FOREIGN KEY ("VendorCategoryId") REFERENCES "VendorCategories" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SaleOrders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SaleOrders" PRIMARY KEY AUTOINCREMENT,
    "SONumber" TEXT NOT NULL,
    "SODate" TEXT NOT NULL,
    "CustomerId" INTEGER NOT NULL,
    "ExpectedDeliveryDate" TEXT NULL,
    "CustomerPONumbers" TEXT NULL,
    "Remarks" TEXT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_SaleOrders_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "InventoryVouchers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InventoryVouchers" PRIMARY KEY AUTOINCREMENT,
    "VoucherNumber" TEXT NOT NULL,
    "VoucherDate" TEXT NOT NULL,
    "VoucherType" INTEGER NOT NULL,
    "InventoryAccountId" INTEGER NOT NULL,
    "ContraAccountId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "ItemAdjustmentNoteId" INTEGER NULL,
    "Narration" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_InventoryVouchers_Accounts_ContraAccountId" FOREIGN KEY ("ContraAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InventoryVouchers_Accounts_InventoryAccountId" FOREIGN KEY ("InventoryAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InventoryVouchers_ItemAdjustmentNotes_ItemAdjustmentNoteId" FOREIGN KEY ("ItemAdjustmentNoteId") REFERENCES "ItemAdjustmentNotes" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ConsumptionVoucherItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConsumptionVoucherItems" PRIMARY KEY AUTOINCREMENT,
    "ConsumptionVoucherId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_ConsumptionVoucherItems_ConsumptionVouchers_ConsumptionVoucherId" FOREIGN KEY ("ConsumptionVoucherId") REFERENCES "ConsumptionVouchers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ConsumptionVoucherItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CustomerProductAliases" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CustomerProductAliases" PRIMARY KEY AUTOINCREMENT,
    "CustomerName" TEXT NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "CustomerProductName" TEXT NULL,
    "CustomerProductCode" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_CustomerProductAliases_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "InternalTransferItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InternalTransferItems" PRIMARY KEY AUTOINCREMENT,
    "InternalTransferId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_InternalTransferItems_InternalTransfers_InternalTransferId" FOREIGN KEY ("InternalTransferId") REFERENCES "InternalTransfers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_InternalTransferItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ItemAdjustmentNoteItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ItemAdjustmentNoteItems" PRIMARY KEY AUTOINCREMENT,
    "ItemAdjustmentNoteId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "AdjustmentType" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_ItemAdjustmentNoteItems_ItemAdjustmentNotes_ItemAdjustmentNoteId" FOREIGN KEY ("ItemAdjustmentNoteId") REFERENCES "ItemAdjustmentNotes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ItemAdjustmentNoteItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ProductPriceHistories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductPriceHistories" PRIMARY KEY AUTOINCREMENT,
    "ProductId" INTEGER NOT NULL,
    "OldCostPrice" decimal(18,2) NOT NULL,
    "NewCostPrice" decimal(18,2) NOT NULL,
    "OldSalePrice" decimal(18,2) NOT NULL,
    "NewSalePrice" decimal(18,2) NOT NULL,
    "ChangedById" TEXT NULL,
    "ChangedByName" TEXT NULL,
    "ChangedOn" TEXT NOT NULL,
    CONSTRAINT "FK_ProductPriceHistories_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);

CREATE TABLE "StockTransactions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StockTransactions" PRIMARY KEY AUTOINCREMENT,
    "ProductId" INTEGER NOT NULL,
    "LocationId" INTEGER NOT NULL,
    "TransactionDate" TEXT NOT NULL,
    "TransactionType" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "ReferenceType" TEXT NOT NULL,
    "ReferenceId" INTEGER NOT NULL,
    "ReferenceNo" TEXT NULL,
    "Remarks" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_StockTransactions_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_StockTransactions_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "IssuedCheques" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_IssuedCheques" PRIMARY KEY AUTOINCREMENT,
    "ChequeReferenceNo" TEXT NOT NULL,
    "ChequeNumber" TEXT NOT NULL,
    "BankName" TEXT NOT NULL,
    "ChequeDate" TEXT NOT NULL,
    "VendorId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    "ClearanceStatus" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_IssuedCheques_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "PurchaseOrders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PurchaseOrders" PRIMARY KEY AUTOINCREMENT,
    "PONumber" TEXT NOT NULL,
    "PODate" TEXT NOT NULL,
    "VendorId" INTEGER NOT NULL,
    "ExpectedDeliveryDate" TEXT NULL,
    "Remarks" TEXT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_PurchaseOrders_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "DeliveryChallans" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DeliveryChallans" PRIMARY KEY AUTOINCREMENT,
    "ChallanNo" TEXT NOT NULL,
    "ChallanDate" TEXT NOT NULL,
    "CustomerId" INTEGER NOT NULL,
    "SaleOrderId" INTEGER NULL,
    "LocationId" INTEGER NOT NULL,
    "VehicleNo" TEXT NULL,
    "DriverName" TEXT NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_DeliveryChallans_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeliveryChallans_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeliveryChallans_SaleOrders_SaleOrderId" FOREIGN KEY ("SaleOrderId") REFERENCES "SaleOrders" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SaleOrderItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SaleOrderItems" PRIMARY KEY AUTOINCREMENT,
    "SaleOrderId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "UnitPrice" decimal(18,2) NOT NULL,
    "SalesTaxRate" decimal(5,2) NULL,
    "LineTotal" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_SaleOrderItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SaleOrderItems_SaleOrders_SaleOrderId" FOREIGN KEY ("SaleOrderId") REFERENCES "SaleOrders" ("Id") ON DELETE CASCADE
);

CREATE TABLE "VendorAdvances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_VendorAdvances" PRIMARY KEY AUTOINCREMENT,
    "AdvanceNumber" TEXT NOT NULL,
    "AdvanceDate" TEXT NOT NULL,
    "VendorId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "PaymentMethod" INTEGER NOT NULL,
    "IssuedChequeId" INTEGER NULL,
    "AdjustedAmount" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_VendorAdvances_IssuedCheques_IssuedChequeId" FOREIGN KEY ("IssuedChequeId") REFERENCES "IssuedCheques" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_VendorAdvances_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "InwardGatePasses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InwardGatePasses" PRIMARY KEY AUTOINCREMENT,
    "GatePassNo" TEXT NOT NULL,
    "GatePassDate" TEXT NOT NULL,
    "VendorId" INTEGER NOT NULL,
    "PurchaseOrderId" INTEGER NULL,
    "VehicleNo" TEXT NULL,
    "DriverName" TEXT NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_InwardGatePasses_PurchaseOrders_PurchaseOrderId" FOREIGN KEY ("PurchaseOrderId") REFERENCES "PurchaseOrders" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InwardGatePasses_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "PurchaseOrderItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PurchaseOrderItems" PRIMARY KEY AUTOINCREMENT,
    "PurchaseOrderId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "UnitPrice" decimal(18,2) NOT NULL,
    "SalesTaxRate" decimal(5,2) NULL,
    "LineTotal" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_PurchaseOrderItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId" FOREIGN KEY ("PurchaseOrderId") REFERENCES "PurchaseOrders" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeliveryChallanItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DeliveryChallanItems" PRIMARY KEY AUTOINCREMENT,
    "DeliveryChallanId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "UnitPrice" decimal(18,2) NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_DeliveryChallanItems_DeliveryChallans_DeliveryChallanId" FOREIGN KEY ("DeliveryChallanId") REFERENCES "DeliveryChallans" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_DeliveryChallanItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SalesInvoices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SalesInvoices" PRIMARY KEY AUTOINCREMENT,
    "InvoiceNumber" TEXT NOT NULL,
    "InvoiceDate" TEXT NOT NULL,
    "DueDate" TEXT NULL,
    "PaymentTermsDays" INTEGER NULL,
    "CustomerId" INTEGER NOT NULL,
    "SaleOrderId" INTEGER NULL,
    "DeliveryChallanId" INTEGER NULL,
    "CustomerPONumbers" TEXT NULL,
    "Remarks" TEXT NULL,
    "SubTotal" decimal(18,2) NOT NULL,
    "TaxAmount" decimal(18,2) NOT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_SalesInvoices_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SalesInvoices_DeliveryChallans_DeliveryChallanId" FOREIGN KEY ("DeliveryChallanId") REFERENCES "DeliveryChallans" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SalesInvoices_SaleOrders_SaleOrderId" FOREIGN KEY ("SaleOrderId") REFERENCES "SaleOrders" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "GoodsReceiptNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_GoodsReceiptNotes" PRIMARY KEY AUTOINCREMENT,
    "GRNNumber" TEXT NOT NULL,
    "GRNDate" TEXT NOT NULL,
    "VendorId" INTEGER NOT NULL,
    "PurchaseOrderId" INTEGER NULL,
    "InwardGatePassId" INTEGER NULL,
    "LocationId" INTEGER NOT NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_GoodsReceiptNotes_InwardGatePasses_InwardGatePassId" FOREIGN KEY ("InwardGatePassId") REFERENCES "InwardGatePasses" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GoodsReceiptNotes_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GoodsReceiptNotes_PurchaseOrders_PurchaseOrderId" FOREIGN KEY ("PurchaseOrderId") REFERENCES "PurchaseOrders" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GoodsReceiptNotes_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "InwardGatePassItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InwardGatePassItems" PRIMARY KEY AUTOINCREMENT,
    "InwardGatePassId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "DeclaredQuantity" decimal(18,3) NOT NULL,
    "UnitCost" decimal(18,2) NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_InwardGatePassItems_InwardGatePasses_InwardGatePassId" FOREIGN KEY ("InwardGatePassId") REFERENCES "InwardGatePasses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_InwardGatePassItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CreditNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CreditNotes" PRIMARY KEY AUTOINCREMENT,
    "CreditNoteNumber" TEXT NOT NULL,
    "CreditNoteDate" TEXT NOT NULL,
    "CustomerId" INTEGER NOT NULL,
    "SalesInvoiceId" INTEGER NULL,
    "DeliveryChallanId" INTEGER NULL,
    "LocationId" INTEGER NULL,
    "Reason" TEXT NULL,
    "Remarks" TEXT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_CreditNotes_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CreditNotes_DeliveryChallans_DeliveryChallanId" FOREIGN KEY ("DeliveryChallanId") REFERENCES "DeliveryChallans" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CreditNotes_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CreditNotes_SalesInvoices_SalesInvoiceId" FOREIGN KEY ("SalesInvoiceId") REFERENCES "SalesInvoices" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SalesInvoiceItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SalesInvoiceItems" PRIMARY KEY AUTOINCREMENT,
    "SalesInvoiceId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "UnitPrice" decimal(18,2) NOT NULL,
    "TaxRate" decimal(5,2) NULL,
    "LineTotal" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_SalesInvoiceItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SalesInvoiceItems_SalesInvoices_SalesInvoiceId" FOREIGN KEY ("SalesInvoiceId") REFERENCES "SalesInvoices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SalesInvoicePayments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SalesInvoicePayments" PRIMARY KEY AUTOINCREMENT,
    "PaymentNumber" TEXT NOT NULL,
    "PaymentDate" TEXT NOT NULL,
    "CustomerId" INTEGER NOT NULL,
    "SalesInvoiceId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "WithholdingTaxRate" decimal(5,2) NULL,
    "WithholdingTaxAmount" decimal(18,2) NOT NULL,
    "NetReceivedAmount" decimal(18,2) NOT NULL,
    "PaymentMethod" INTEGER NOT NULL,
    "ChequeNumber" TEXT NULL,
    "BankName" TEXT NULL,
    "ClearanceStatus" INTEGER NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_SalesInvoicePayments_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SalesInvoicePayments_SalesInvoices_SalesInvoiceId" FOREIGN KEY ("SalesInvoiceId") REFERENCES "SalesInvoices" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "GoodsReceiptNoteItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_GoodsReceiptNoteItems" PRIMARY KEY AUTOINCREMENT,
    "GoodsReceiptNoteId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "DeclaredQuantity" decimal(18,3) NOT NULL,
    "AcceptedQuantity" decimal(18,3) NOT NULL,
    "RejectedQuantity" decimal(18,3) NOT NULL,
    "UnitCost" decimal(18,2) NOT NULL,
    "LineTotal" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_GoodsReceiptNoteItems_GoodsReceiptNotes_GoodsReceiptNoteId" FOREIGN KEY ("GoodsReceiptNoteId") REFERENCES "GoodsReceiptNotes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GoodsReceiptNoteItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "PurchaseInvoices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PurchaseInvoices" PRIMARY KEY AUTOINCREMENT,
    "InvoiceNumber" TEXT NOT NULL,
    "VendorInvoiceNumber" TEXT NULL,
    "InvoiceDate" TEXT NOT NULL,
    "DueDate" TEXT NULL,
    "VendorId" INTEGER NOT NULL,
    "PurchaseOrderId" INTEGER NULL,
    "GoodsReceiptNoteId" INTEGER NULL,
    "Remarks" TEXT NULL,
    "SubTotal" decimal(18,2) NOT NULL,
    "TaxAmount" decimal(18,2) NOT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "WithholdingTaxRate" decimal(5,2) NULL,
    "WithholdingTaxAmount" decimal(18,2) NOT NULL,
    "NetPayableAmount" decimal(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_PurchaseInvoices_GoodsReceiptNotes_GoodsReceiptNoteId" FOREIGN KEY ("GoodsReceiptNoteId") REFERENCES "GoodsReceiptNotes" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PurchaseInvoices_PurchaseOrders_PurchaseOrderId" FOREIGN KEY ("PurchaseOrderId") REFERENCES "PurchaseOrders" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PurchaseInvoices_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CreditNoteItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CreditNoteItems" PRIMARY KEY AUTOINCREMENT,
    "CreditNoteId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "UnitPrice" decimal(18,2) NOT NULL,
    "SalesTaxRate" decimal(5,2) NULL,
    "LineTotal" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_CreditNoteItems_CreditNotes_CreditNoteId" FOREIGN KEY ("CreditNoteId") REFERENCES "CreditNotes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CreditNoteItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "DebitNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DebitNotes" PRIMARY KEY AUTOINCREMENT,
    "DebitNoteNumber" TEXT NOT NULL,
    "DebitNoteDate" TEXT NOT NULL,
    "VendorId" INTEGER NOT NULL,
    "PurchaseInvoiceId" INTEGER NULL,
    "GoodsReceiptNoteId" INTEGER NULL,
    "LocationId" INTEGER NULL,
    "Reason" TEXT NULL,
    "Remarks" TEXT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_DebitNotes_GoodsReceiptNotes_GoodsReceiptNoteId" FOREIGN KEY ("GoodsReceiptNoteId") REFERENCES "GoodsReceiptNotes" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DebitNotes_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DebitNotes_PurchaseInvoices_PurchaseInvoiceId" FOREIGN KEY ("PurchaseInvoiceId") REFERENCES "PurchaseInvoices" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DebitNotes_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "PurchaseInvoiceItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PurchaseInvoiceItems" PRIMARY KEY AUTOINCREMENT,
    "PurchaseInvoiceId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "UnitPrice" decimal(18,2) NOT NULL,
    "TaxRate" decimal(5,2) NULL,
    "LineTotal" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_PurchaseInvoiceItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PurchaseInvoiceItems_PurchaseInvoices_PurchaseInvoiceId" FOREIGN KEY ("PurchaseInvoiceId") REFERENCES "PurchaseInvoices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PurchaseInvoicePayments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PurchaseInvoicePayments" PRIMARY KEY AUTOINCREMENT,
    "PaymentNumber" TEXT NOT NULL,
    "PaymentDate" TEXT NOT NULL,
    "VendorId" INTEGER NOT NULL,
    "PurchaseInvoiceId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "WithholdingTaxRate" decimal(5,2) NULL,
    "WithholdingTaxAmount" decimal(18,2) NOT NULL,
    "NetPaymentAmount" decimal(18,2) NOT NULL,
    "PaymentMethod" INTEGER NOT NULL,
    "IssuedChequeId" INTEGER NULL,
    "VendorAdvanceId" INTEGER NULL,
    "Remarks" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_PurchaseInvoicePayments_IssuedCheques_IssuedChequeId" FOREIGN KEY ("IssuedChequeId") REFERENCES "IssuedCheques" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PurchaseInvoicePayments_PurchaseInvoices_PurchaseInvoiceId" FOREIGN KEY ("PurchaseInvoiceId") REFERENCES "PurchaseInvoices" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PurchaseInvoicePayments_VendorAdvances_VendorAdvanceId" FOREIGN KEY ("VendorAdvanceId") REFERENCES "VendorAdvances" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PurchaseInvoicePayments_Vendors_VendorId" FOREIGN KEY ("VendorId") REFERENCES "Vendors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "DebitNoteItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DebitNoteItems" PRIMARY KEY AUTOINCREMENT,
    "DebitNoteId" INTEGER NOT NULL,
    "ProductId" INTEGER NOT NULL,
    "Quantity" decimal(18,3) NOT NULL,
    "UnitPrice" decimal(18,2) NOT NULL,
    "SalesTaxRate" decimal(5,2) NULL,
    "LineTotal" decimal(18,2) NOT NULL,
    "Remarks" TEXT NULL,
    CONSTRAINT "FK_DebitNoteItems_DebitNotes_DebitNoteId" FOREIGN KEY ("DebitNoteId") REFERENCES "DebitNotes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_DebitNoteItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AttendanceRecords" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AttendanceRecords" PRIMARY KEY AUTOINCREMENT,
    "EmployeeId" INTEGER NOT NULL,
    "Date" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "CheckInTime" TEXT NULL,
    "CheckOutTime" TEXT NULL,
    "IsLate" INTEGER NOT NULL,
    "LateByMinutes" INTEGER NOT NULL,
    "IsEarlyDeparture" INTEGER NOT NULL,
    "EarlyByMinutes" INTEGER NOT NULL,
    "Remarks" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_AttendanceRecords_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Departments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Departments" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Code" TEXT NULL,
    "ParentDepartmentId" INTEGER NULL,
    "HeadEmployeeId" INTEGER NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_Departments_Departments_ParentDepartmentId" FOREIGN KEY ("ParentDepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Departments_Employees_HeadEmployeeId" FOREIGN KEY ("HeadEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Employees" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Employees" PRIMARY KEY AUTOINCREMENT,
    "EmployeeCode" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Department" TEXT NULL,
    "DepartmentId" INTEGER NULL,
    "ShiftTimingId" INTEGER NULL,
    "Designation" TEXT NULL,
    "CNICNumber" TEXT NULL,
    "ContactCountryCode" TEXT NULL,
    "Phone" TEXT NULL,
    "Email" TEXT NULL,
    "Address" TEXT NULL,
    "ZipCode" TEXT NULL,
    "ProfileImagePath" TEXT NULL,
    "MonthlySalary" decimal(18,2) NULL,
    "BasicSalary" decimal(18,2) NOT NULL,
    "HouseRentAllowance" decimal(18,2) NOT NULL,
    "MedicalAllowance" decimal(18,2) NOT NULL,
    "OtherAllowances" decimal(18,2) NOT NULL,
    "BankName" TEXT NULL,
    "BankAccountNumber" TEXT NULL,
    "TaxNumber" TEXT NULL,
    "EOBINumber" TEXT NULL,
    "JoiningDate" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_Employees_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Employees_ShiftTimings_ShiftTimingId" FOREIGN KEY ("ShiftTimingId") REFERENCES "ShiftTimings" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "LeaveEncashmentRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_LeaveEncashmentRequests" PRIMARY KEY AUTOINCREMENT,
    "RequestNumber" TEXT NOT NULL,
    "EmployeeId" INTEGER NOT NULL,
    "LeaveTypeId" INTEGER NOT NULL,
    "DaysToEncash" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "Reason" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_LeaveEncashmentRequests_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_LeaveEncashmentRequests_LeaveTypes_LeaveTypeId" FOREIGN KEY ("LeaveTypeId") REFERENCES "LeaveTypes" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "LeaveRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_LeaveRequests" PRIMARY KEY AUTOINCREMENT,
    "RequestNumber" TEXT NOT NULL,
    "EmployeeId" INTEGER NOT NULL,
    "LeaveTypeId" INTEGER NOT NULL,
    "FromDate" TEXT NOT NULL,
    "ToDate" TEXT NOT NULL,
    "TotalDays" INTEGER NOT NULL,
    "Reason" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_LeaveRequests_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_LeaveRequests_LeaveTypes_LeaveTypeId" FOREIGN KEY ("LeaveTypeId") REFERENCES "LeaveTypes" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "LoanAdvanceRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_LoanAdvanceRequests" PRIMARY KEY AUTOINCREMENT,
    "RequestNumber" TEXT NOT NULL,
    "EmployeeId" INTEGER NOT NULL,
    "RequestType" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "InstallmentMonths" INTEGER NOT NULL,
    "Reason" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_LoanAdvanceRequests_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "MidSalaryRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MidSalaryRequests" PRIMARY KEY AUTOINCREMENT,
    "RequestNumber" TEXT NOT NULL,
    "EmployeeId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "Reason" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_MidSalaryRequests_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Payslips" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Payslips" PRIMARY KEY AUTOINCREMENT,
    "PayslipNumber" TEXT NOT NULL,
    "PayRunId" INTEGER NULL,
    "EmployeeId" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "BasicSalary" decimal(18,2) NOT NULL,
    "HouseRentAllowance" decimal(18,2) NOT NULL,
    "MedicalAllowance" decimal(18,2) NOT NULL,
    "OtherAllowances" decimal(18,2) NOT NULL,
    "GrossSalary" decimal(18,2) NOT NULL,
    "WorkingDays" INTEGER NOT NULL,
    "OvertimeHours" decimal(10,2) NOT NULL,
    "LateHours" decimal(10,2) NOT NULL,
    "NetHours" decimal(10,2) NOT NULL,
    "OvertimeAmount" decimal(18,2) NOT NULL,
    "AdvancePreviousBalance" decimal(18,2) NOT NULL,
    "CurrentAdvanceGiven" decimal(18,2) NOT NULL,
    "TotalAdvance" decimal(18,2) NOT NULL,
    "AdvanceDeducted" decimal(18,2) NOT NULL,
    "AdvanceRemainingBalance" decimal(18,2) NOT NULL,
    "FixedMonthlyDeduction" decimal(18,2) NOT NULL,
    "IncomeTaxDeduction" decimal(18,2) NOT NULL,
    "StatutoryDeductions" decimal(18,2) NOT NULL,
    "LoanDeduction" decimal(18,2) NOT NULL,
    "AbsentDeduction" decimal(18,2) NOT NULL,
    "OtherDeductions" decimal(18,2) NOT NULL,
    "TotalDeductions" decimal(18,2) NOT NULL,
    "NetPay" decimal(18,2) NOT NULL,
    "PresentDays" INTEGER NOT NULL,
    "AbsentDays" INTEGER NOT NULL,
    "LeaveDays" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_Payslips_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Payslips_PayRuns_PayRunId" FOREIGN KEY ("PayRunId") REFERENCES "PayRuns" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ReimbursementRequests" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ReimbursementRequests" PRIMARY KEY AUTOINCREMENT,
    "RequestNumber" TEXT NOT NULL,
    "EmployeeId" INTEGER NOT NULL,
    "ReimbursementTypeId" INTEGER NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "ExpenseDate" TEXT NOT NULL,
    "Description" TEXT NULL,
    "ReceiptImagePath" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "ApprovedOn" TEXT NULL,
    "RejectionReason" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_ReimbursementRequests_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ReimbursementRequests_ReimbursementTypes_ReimbursementTypeId" FOREIGN KEY ("ReimbursementTypeId") REFERENCES "ReimbursementTypes" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SalaryIncrements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SalaryIncrements" PRIMARY KEY AUTOINCREMENT,
    "EmployeeId" INTEGER NOT NULL,
    "EffectiveDate" TEXT NOT NULL,
    "OldBasicSalary" decimal(18,2) NOT NULL,
    "NewBasicSalary" decimal(18,2) NOT NULL,
    "Reason" TEXT NULL,
    "ApprovedById" TEXT NULL,
    "ApprovedByName" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    CONSTRAINT "FK_SalaryIncrements_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_Accounts_AccountCode" ON "Accounts" ("AccountCode");

CREATE INDEX "IX_Accounts_ParentAccountId" ON "Accounts" ("ParentAccountId");

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE UNIQUE INDEX "IX_AttendanceRecords_EmployeeId_Date" ON "AttendanceRecords" ("EmployeeId", "Date");

CREATE INDEX "IX_BankPaymentVouchers_BankAccountId" ON "BankPaymentVouchers" ("BankAccountId");

CREATE INDEX "IX_BankPaymentVouchers_ContraAccountId" ON "BankPaymentVouchers" ("ContraAccountId");

CREATE UNIQUE INDEX "IX_BankPaymentVouchers_VoucherNumber" ON "BankPaymentVouchers" ("VoucherNumber");

CREATE INDEX "IX_BankReceiptVouchers_BankAccountId" ON "BankReceiptVouchers" ("BankAccountId");

CREATE INDEX "IX_BankReceiptVouchers_ContraAccountId" ON "BankReceiptVouchers" ("ContraAccountId");

CREATE UNIQUE INDEX "IX_BankReceiptVouchers_VoucherNumber" ON "BankReceiptVouchers" ("VoucherNumber");

CREATE INDEX "IX_CashPaymentVouchers_CashAccountId" ON "CashPaymentVouchers" ("CashAccountId");

CREATE INDEX "IX_CashPaymentVouchers_ContraAccountId" ON "CashPaymentVouchers" ("ContraAccountId");

CREATE UNIQUE INDEX "IX_CashPaymentVouchers_VoucherNumber" ON "CashPaymentVouchers" ("VoucherNumber");

CREATE INDEX "IX_CashReceiptVouchers_CashAccountId" ON "CashReceiptVouchers" ("CashAccountId");

CREATE INDEX "IX_CashReceiptVouchers_ContraAccountId" ON "CashReceiptVouchers" ("ContraAccountId");

CREATE UNIQUE INDEX "IX_CashReceiptVouchers_VoucherNumber" ON "CashReceiptVouchers" ("VoucherNumber");

CREATE INDEX "IX_ConsumptionVoucherItems_ConsumptionVoucherId" ON "ConsumptionVoucherItems" ("ConsumptionVoucherId");

CREATE INDEX "IX_ConsumptionVoucherItems_ProductId" ON "ConsumptionVoucherItems" ("ProductId");

CREATE INDEX "IX_ConsumptionVouchers_LocationId" ON "ConsumptionVouchers" ("LocationId");

CREATE UNIQUE INDEX "IX_ConsumptionVouchers_VoucherNo" ON "ConsumptionVouchers" ("VoucherNo");

CREATE INDEX "IX_CreditNoteItems_CreditNoteId" ON "CreditNoteItems" ("CreditNoteId");

CREATE INDEX "IX_CreditNoteItems_ProductId" ON "CreditNoteItems" ("ProductId");

CREATE UNIQUE INDEX "IX_CreditNotes_CreditNoteNumber" ON "CreditNotes" ("CreditNoteNumber");

CREATE INDEX "IX_CreditNotes_CustomerId" ON "CreditNotes" ("CustomerId");

CREATE INDEX "IX_CreditNotes_DeliveryChallanId" ON "CreditNotes" ("DeliveryChallanId");

CREATE INDEX "IX_CreditNotes_LocationId" ON "CreditNotes" ("LocationId");

CREATE INDEX "IX_CreditNotes_SalesInvoiceId" ON "CreditNotes" ("SalesInvoiceId");

CREATE INDEX "IX_CustomerProductAliases_CustomerName_ProductId" ON "CustomerProductAliases" ("CustomerName", "ProductId");

CREATE INDEX "IX_CustomerProductAliases_ProductId" ON "CustomerProductAliases" ("ProductId");

CREATE INDEX "IX_Customers_CustomerCategoryId" ON "Customers" ("CustomerCategoryId");

CREATE UNIQUE INDEX "IX_Customers_CustomerCode" ON "Customers" ("CustomerCode");

CREATE INDEX "IX_DebitNoteItems_DebitNoteId" ON "DebitNoteItems" ("DebitNoteId");

CREATE INDEX "IX_DebitNoteItems_ProductId" ON "DebitNoteItems" ("ProductId");

CREATE UNIQUE INDEX "IX_DebitNotes_DebitNoteNumber" ON "DebitNotes" ("DebitNoteNumber");

CREATE INDEX "IX_DebitNotes_GoodsReceiptNoteId" ON "DebitNotes" ("GoodsReceiptNoteId");

CREATE INDEX "IX_DebitNotes_LocationId" ON "DebitNotes" ("LocationId");

CREATE INDEX "IX_DebitNotes_PurchaseInvoiceId" ON "DebitNotes" ("PurchaseInvoiceId");

CREATE INDEX "IX_DebitNotes_VendorId" ON "DebitNotes" ("VendorId");

CREATE INDEX "IX_DeliveryChallanItems_DeliveryChallanId" ON "DeliveryChallanItems" ("DeliveryChallanId");

CREATE INDEX "IX_DeliveryChallanItems_ProductId" ON "DeliveryChallanItems" ("ProductId");

CREATE UNIQUE INDEX "IX_DeliveryChallans_ChallanNo" ON "DeliveryChallans" ("ChallanNo");

CREATE INDEX "IX_DeliveryChallans_CustomerId" ON "DeliveryChallans" ("CustomerId");

CREATE INDEX "IX_DeliveryChallans_LocationId" ON "DeliveryChallans" ("LocationId");

CREATE INDEX "IX_DeliveryChallans_SaleOrderId" ON "DeliveryChallans" ("SaleOrderId");

CREATE INDEX "IX_Departments_HeadEmployeeId" ON "Departments" ("HeadEmployeeId");

CREATE INDEX "IX_Departments_ParentDepartmentId" ON "Departments" ("ParentDepartmentId");

CREATE INDEX "IX_Employees_DepartmentId" ON "Employees" ("DepartmentId");

CREATE UNIQUE INDEX "IX_Employees_EmployeeCode" ON "Employees" ("EmployeeCode");

CREATE INDEX "IX_Employees_ShiftTimingId" ON "Employees" ("ShiftTimingId");

CREATE INDEX "IX_GoodsReceiptNoteItems_GoodsReceiptNoteId" ON "GoodsReceiptNoteItems" ("GoodsReceiptNoteId");

CREATE INDEX "IX_GoodsReceiptNoteItems_ProductId" ON "GoodsReceiptNoteItems" ("ProductId");

CREATE UNIQUE INDEX "IX_GoodsReceiptNotes_GRNNumber" ON "GoodsReceiptNotes" ("GRNNumber");

CREATE INDEX "IX_GoodsReceiptNotes_InwardGatePassId" ON "GoodsReceiptNotes" ("InwardGatePassId");

CREATE INDEX "IX_GoodsReceiptNotes_LocationId" ON "GoodsReceiptNotes" ("LocationId");

CREATE INDEX "IX_GoodsReceiptNotes_PurchaseOrderId" ON "GoodsReceiptNotes" ("PurchaseOrderId");

CREATE INDEX "IX_GoodsReceiptNotes_VendorId" ON "GoodsReceiptNotes" ("VendorId");

CREATE INDEX "IX_InternalTransferItems_InternalTransferId" ON "InternalTransferItems" ("InternalTransferId");

CREATE INDEX "IX_InternalTransferItems_ProductId" ON "InternalTransferItems" ("ProductId");

CREATE INDEX "IX_InternalTransfers_FromLocationId" ON "InternalTransfers" ("FromLocationId");

CREATE INDEX "IX_InternalTransfers_ToLocationId" ON "InternalTransfers" ("ToLocationId");

CREATE UNIQUE INDEX "IX_InternalTransfers_TransferNo" ON "InternalTransfers" ("TransferNo");

CREATE INDEX "IX_InventoryVouchers_ContraAccountId" ON "InventoryVouchers" ("ContraAccountId");

CREATE INDEX "IX_InventoryVouchers_InventoryAccountId" ON "InventoryVouchers" ("InventoryAccountId");

CREATE INDEX "IX_InventoryVouchers_ItemAdjustmentNoteId" ON "InventoryVouchers" ("ItemAdjustmentNoteId");

CREATE UNIQUE INDEX "IX_InventoryVouchers_VoucherNumber" ON "InventoryVouchers" ("VoucherNumber");

CREATE UNIQUE INDEX "IX_InwardGatePasses_GatePassNo" ON "InwardGatePasses" ("GatePassNo");

CREATE INDEX "IX_InwardGatePasses_PurchaseOrderId" ON "InwardGatePasses" ("PurchaseOrderId");

CREATE INDEX "IX_InwardGatePasses_VendorId" ON "InwardGatePasses" ("VendorId");

CREATE INDEX "IX_InwardGatePassItems_InwardGatePassId" ON "InwardGatePassItems" ("InwardGatePassId");

CREATE INDEX "IX_InwardGatePassItems_ProductId" ON "InwardGatePassItems" ("ProductId");

CREATE UNIQUE INDEX "IX_IssuedCheques_ChequeReferenceNo" ON "IssuedCheques" ("ChequeReferenceNo");

CREATE INDEX "IX_IssuedCheques_VendorId" ON "IssuedCheques" ("VendorId");

CREATE INDEX "IX_ItemAdjustmentNoteItems_ItemAdjustmentNoteId" ON "ItemAdjustmentNoteItems" ("ItemAdjustmentNoteId");

CREATE INDEX "IX_ItemAdjustmentNoteItems_ProductId" ON "ItemAdjustmentNoteItems" ("ProductId");

CREATE UNIQUE INDEX "IX_ItemAdjustmentNotes_AdjustmentNo" ON "ItemAdjustmentNotes" ("AdjustmentNo");

CREATE INDEX "IX_ItemAdjustmentNotes_LocationId" ON "ItemAdjustmentNotes" ("LocationId");

CREATE UNIQUE INDEX "IX_JournalEntries_EntryNumber" ON "JournalEntries" ("EntryNumber");

CREATE INDEX "IX_JournalEntryLines_AccountId" ON "JournalEntryLines" ("AccountId");

CREATE INDEX "IX_JournalEntryLines_JournalEntryId" ON "JournalEntryLines" ("JournalEntryId");

CREATE INDEX "IX_JournalVoucherLines_AccountId" ON "JournalVoucherLines" ("AccountId");

CREATE INDEX "IX_JournalVoucherLines_JournalVoucherId" ON "JournalVoucherLines" ("JournalVoucherId");

CREATE UNIQUE INDEX "IX_JournalVouchers_VoucherNumber" ON "JournalVouchers" ("VoucherNumber");

CREATE INDEX "IX_LeaveEncashmentRequests_EmployeeId" ON "LeaveEncashmentRequests" ("EmployeeId");

CREATE INDEX "IX_LeaveEncashmentRequests_LeaveTypeId" ON "LeaveEncashmentRequests" ("LeaveTypeId");

CREATE UNIQUE INDEX "IX_LeaveEncashmentRequests_RequestNumber" ON "LeaveEncashmentRequests" ("RequestNumber");

CREATE INDEX "IX_LeaveRequests_EmployeeId" ON "LeaveRequests" ("EmployeeId");

CREATE INDEX "IX_LeaveRequests_LeaveTypeId" ON "LeaveRequests" ("LeaveTypeId");

CREATE UNIQUE INDEX "IX_LeaveRequests_RequestNumber" ON "LeaveRequests" ("RequestNumber");

CREATE INDEX "IX_LoanAdvanceRequests_EmployeeId" ON "LoanAdvanceRequests" ("EmployeeId");

CREATE UNIQUE INDEX "IX_LoanAdvanceRequests_RequestNumber" ON "LoanAdvanceRequests" ("RequestNumber");

CREATE INDEX "IX_MidSalaryRequests_EmployeeId" ON "MidSalaryRequests" ("EmployeeId");

CREATE UNIQUE INDEX "IX_MidSalaryRequests_RequestNumber" ON "MidSalaryRequests" ("RequestNumber");

CREATE UNIQUE INDEX "IX_PayRuns_RunNumber" ON "PayRuns" ("RunNumber");

CREATE INDEX "IX_Payslips_EmployeeId" ON "Payslips" ("EmployeeId");

CREATE INDEX "IX_Payslips_PayRunId" ON "Payslips" ("PayRunId");

CREATE UNIQUE INDEX "IX_Payslips_PayslipNumber" ON "Payslips" ("PayslipNumber");

CREATE INDEX "IX_ProductPriceHistories_ProductId" ON "ProductPriceHistories" ("ProductId");

CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");

CREATE INDEX "IX_Products_DefaultLocationId" ON "Products" ("DefaultLocationId");

CREATE UNIQUE INDEX "IX_Products_Sku" ON "Products" ("Sku");

CREATE INDEX "IX_Products_UnitId" ON "Products" ("UnitId");

CREATE INDEX "IX_PurchaseInvoiceItems_ProductId" ON "PurchaseInvoiceItems" ("ProductId");

CREATE INDEX "IX_PurchaseInvoiceItems_PurchaseInvoiceId" ON "PurchaseInvoiceItems" ("PurchaseInvoiceId");

CREATE INDEX "IX_PurchaseInvoicePayments_IssuedChequeId" ON "PurchaseInvoicePayments" ("IssuedChequeId");

CREATE UNIQUE INDEX "IX_PurchaseInvoicePayments_PaymentNumber" ON "PurchaseInvoicePayments" ("PaymentNumber");

CREATE INDEX "IX_PurchaseInvoicePayments_PurchaseInvoiceId" ON "PurchaseInvoicePayments" ("PurchaseInvoiceId");

CREATE INDEX "IX_PurchaseInvoicePayments_VendorAdvanceId" ON "PurchaseInvoicePayments" ("VendorAdvanceId");

CREATE INDEX "IX_PurchaseInvoicePayments_VendorId" ON "PurchaseInvoicePayments" ("VendorId");

CREATE INDEX "IX_PurchaseInvoices_GoodsReceiptNoteId" ON "PurchaseInvoices" ("GoodsReceiptNoteId");

CREATE UNIQUE INDEX "IX_PurchaseInvoices_InvoiceNumber" ON "PurchaseInvoices" ("InvoiceNumber");

CREATE INDEX "IX_PurchaseInvoices_PurchaseOrderId" ON "PurchaseInvoices" ("PurchaseOrderId");

CREATE INDEX "IX_PurchaseInvoices_VendorId" ON "PurchaseInvoices" ("VendorId");

CREATE INDEX "IX_PurchaseOrderItems_ProductId" ON "PurchaseOrderItems" ("ProductId");

CREATE INDEX "IX_PurchaseOrderItems_PurchaseOrderId" ON "PurchaseOrderItems" ("PurchaseOrderId");

CREATE UNIQUE INDEX "IX_PurchaseOrders_PONumber" ON "PurchaseOrders" ("PONumber");

CREATE INDEX "IX_PurchaseOrders_VendorId" ON "PurchaseOrders" ("VendorId");

CREATE INDEX "IX_ReimbursementRequests_EmployeeId" ON "ReimbursementRequests" ("EmployeeId");

CREATE INDEX "IX_ReimbursementRequests_ReimbursementTypeId" ON "ReimbursementRequests" ("ReimbursementTypeId");

CREATE UNIQUE INDEX "IX_ReimbursementRequests_RequestNumber" ON "ReimbursementRequests" ("RequestNumber");

CREATE INDEX "IX_SalaryIncrements_EmployeeId" ON "SalaryIncrements" ("EmployeeId");

CREATE INDEX "IX_SaleOrderItems_ProductId" ON "SaleOrderItems" ("ProductId");

CREATE INDEX "IX_SaleOrderItems_SaleOrderId" ON "SaleOrderItems" ("SaleOrderId");

CREATE INDEX "IX_SaleOrders_CustomerId" ON "SaleOrders" ("CustomerId");

CREATE UNIQUE INDEX "IX_SaleOrders_SONumber" ON "SaleOrders" ("SONumber");

CREATE INDEX "IX_SalesInvoiceItems_ProductId" ON "SalesInvoiceItems" ("ProductId");

CREATE INDEX "IX_SalesInvoiceItems_SalesInvoiceId" ON "SalesInvoiceItems" ("SalesInvoiceId");

CREATE INDEX "IX_SalesInvoicePayments_CustomerId" ON "SalesInvoicePayments" ("CustomerId");

CREATE UNIQUE INDEX "IX_SalesInvoicePayments_PaymentNumber" ON "SalesInvoicePayments" ("PaymentNumber");

CREATE INDEX "IX_SalesInvoicePayments_SalesInvoiceId" ON "SalesInvoicePayments" ("SalesInvoiceId");

CREATE INDEX "IX_SalesInvoices_CustomerId" ON "SalesInvoices" ("CustomerId");

CREATE INDEX "IX_SalesInvoices_DeliveryChallanId" ON "SalesInvoices" ("DeliveryChallanId");

CREATE UNIQUE INDEX "IX_SalesInvoices_InvoiceNumber" ON "SalesInvoices" ("InvoiceNumber");

CREATE INDEX "IX_SalesInvoices_SaleOrderId" ON "SalesInvoices" ("SaleOrderId");

CREATE INDEX "IX_StockTransactions_LocationId" ON "StockTransactions" ("LocationId");

CREATE INDEX "IX_StockTransactions_ProductId_LocationId" ON "StockTransactions" ("ProductId", "LocationId");

CREATE UNIQUE INDEX "IX_VendorAdvances_AdvanceNumber" ON "VendorAdvances" ("AdvanceNumber");

CREATE INDEX "IX_VendorAdvances_IssuedChequeId" ON "VendorAdvances" ("IssuedChequeId");

CREATE INDEX "IX_VendorAdvances_VendorId" ON "VendorAdvances" ("VendorId");

CREATE INDEX "IX_Vendors_VendorCategoryId" ON "Vendors" ("VendorCategoryId");

CREATE UNIQUE INDEX "IX_Vendors_VendorCode" ON "Vendors" ("VendorCode");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260919165919_InitialCreate', '10.0.12');

COMMIT;

