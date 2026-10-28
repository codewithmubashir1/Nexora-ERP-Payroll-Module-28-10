# ERP Identity Starter (ASP.NET Core MVC + Identity + AdminLTE 4)

This is the **Identity / access-management shell** for your ERP system. It gives you
a working login, roles, user management and a themed AdminLTE 4 shell with the
Sales / Purchases / Inventory / Accounts / Payroll / Reports / Chart of Accounts /
Company / Settings / My Account modules already wired into the sidebar as
placeholder pages, ready for you (or Codex) to build out.

## Stack

- ASP.NET Core 10 MVC + ASP.NET Core Identity
- EF Core with SQLite (zero-config, file-based — swap for SQL Server/Postgres later if you want)
- AdminLTE 4 (Bootstrap 5.3, no jQuery core, CDN-loaded)
- Bootstrap 5, Font Awesome 6, Animate.css
- DataTables, Select2, intl-tel-input, Flatpickr (custom date picker), SweetAlert2, Notyf

All front-end libraries are loaded via CDN (jsDelivr / cdnjs) — there is no `npm install`
step required to run the app.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (matching the `net10.0` target in
  `ERP.Identity.csproj` — if you're on an earlier SDK, change `<TargetFramework>` to
  `net9.0` or `net8.0` and adjust package versions accordingly)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef` (if not already installed)

## Getting started

```bash
cd ERP.Identity

# restore & build
dotnet restore
dotnet build

# create the database (SQLite file at App_Data/erp_identity.db)
dotnet ef migrations add InitialCreate
dotnet ef database update

# run
dotnet run
```

The app starts at the **Login page** (`/Account/Login`) — there is no navbar/sidebar on
this page, exactly as requested. On successful login you're redirected to `/Dashboard`,
which uses the full AdminLTE layout (sidebar + navbar).

> Note: `Program.cs` also calls `db.Database.Migrate()` automatically on startup, so once
> you've created the first migration, subsequent `dotnet run` calls will apply any pending
> migrations for you.

## Exporting and reusing the schema

The complete SQLite schema is exported to `Database/erp_identity_schema.sql`. It is generated
from the EF Core migrations and includes the ERP tables, ASP.NET Identity tables, foreign keys,
indexes, and the migration history table.

To regenerate it after changing a model or adding a migration:

```bash
dotnet ef migrations add YourMigrationName
dotnet ef migrations script --no-build --output Database/erp_identity_schema.sql
```

The script is a one-time creation script for a fresh SQLite database. Apply it with the SQLite
CLI or a SQLite library, then point the other project at that database. For an existing database,
copy the migrations and run `dotnet ef database update` instead. SQLite does not support EF Core's
idempotent migration script generation.

See `Database/README.md` for the reuse workflow and provider limitation.

## Default admin login

Seeded automatically the first time the app runs (see `appsettings.json` → `SeedAdmin`):

```
Email:    admin@erp.local
Password: Admin@12345
```

**Change this password (or the seed values in `appsettings.json`) before deploying anywhere real.**

## Roles

Defined in `Models/AppRoles.cs` and seeded on startup:

- Admin
- Supervisor
- Manager
- Standard User
- Accountant
- Store Incharge

Only **Admin** can access `Users & Roles` (`/Users`) to create/edit/deactivate users and
assign roles. Add `[Authorize(Roles = "...")]` to any placeholder controller to restrict
a module to specific roles once you build it out.

## What's implemented

- **Login page as the default route**, no navbar/sidebar, redirects to Dashboard on success
  (`Views/Account/Login.cshtml` + `Views/Shared/_LoginLayout.cshtml`)
- **Custom Identity fields** on `ApplicationUser` (EmployeeCode, Designation, Department,
  Gender, DateOfBirth, JoiningDate, NationalId, Address/City/Country, Emergency contact,
  Bio, ProfileImagePath, IsActive, CreatedOn, LastLoginOn)
- **Real-time avatar upload with preview** — `Views/Shared/_AvatarUpload.cshtml` +
  `wwwroot/js/site.js` (FileReader-based live preview, default avatar SVG placeholder,
  3MB/type validation client + server side via `Services/IFileUploadService.cs`)
- **Extended "My Account" profile page** (`/Profile`) with all the extra fields above,
  phone number via intl-tel-input, dates via Flatpickr, dropdowns via Select2
- **Six ERP user roles**, seeded on startup, assignable from the Users admin screen
- **Notyf toast notifications** for success/error (server messages flow through TempData →
  `#serverToast` → Notyf on page load; see `site.js`)
- **SweetAlert2 confirmation modals** for logout and for deleting a user
  (`confirmDelete()` helper in `site.js`, reusable for future modules)
- **Animated, hoverable navbar dropdowns** (quick module launcher + user menu) using
  Animate.css classes toggled on hover (`.hoverable-dropdown` in `styles.css`/`site.js`)
- **Flatpickr** as the custom date picker (used on DOB/Joining Date fields, replacing the
  native browser date input)
- **DataTables** powering the Users grid (`/Users`) via an AJAX JSON endpoint
  (`UsersController.GetAll`)
- AdminLTE 4 sidebar with links already stubbed to: Dashboard, Sales, Purchases, Inventory,
  Accounts, Payroll, Chart of Accounts, Reports, Users & Roles (admin only), Company,
  Settings, My Account

## Inventory module

Fully working CRUD module, interlinked end-to-end:

- **Products** (`/Inventory/Products`) — SKU auto-generated (`SKU-000001...`), manual product
  code, name, Category + Unit dropdowns (Select2), cost/sale price, tax rate, reorder level,
  opening stock, default location, description, image upload with live preview. The list shows
  live current stock (computed from the ledger) with a red badge when at/below reorder level.
- **Product Categories** (`/Inventory/Categories`) — simple lookup CRUD, blocks delete while
  referenced by a product.
- **Locations** (`/Inventory/Locations`) — warehouses/stores, blocks delete while referenced by
  any product or stock movement.
- **Internal Transfer** (`/Inventory/Transfers`) — header (From/To location, date) + dynamic
  product line items; posts a TransferOut + TransferIn pair to the stock ledger per line.
- **Item Adjustment Note** (`/Inventory/Adjustments`) — header (location, reason) + dynamic line
  items, each with its own Increase/Decrease type and quantity.
- **Consumption Voucher** (`/Inventory/Consumption`) — header (location, department, issued to,
  purpose) + dynamic line items; always reduces stock.

All three transaction documents post to a single `StockTransaction` ledger
(`Models/Inventory/StockTransaction.cs`) via `IStockService` — current stock for any
product/location is always `SUM(Quantity)` over that ledger, so nothing gets out of sync between
documents. Units of Measure are seeded by `DbInitializer` (Pcs, Kg, Litre, Box, Carton, Dozen,
Meter, Pack) and used as a plain dropdown; add a CRUD screen for them later following the same
pattern as Categories/Locations if you want them user-manageable.

Posted transaction documents (Transfer/Adjustment/Consumption) are intentionally **not
deletable** once posted, to keep the stock ledger auditable — post a reversing document instead.
Products and lookups *are* deletable, but only while they have no stock activity.

## Default theme, import/export, and approval workflow (latest update)

- **Color palette**: centralized in `wwwroot/css/styles.css`, which is the only application
  stylesheet linked by the authenticated and login layouts.
- **DataTables are now fully featured** everywhere via a single global default
  (`wwwroot/js/site.js`): export to Copy/CSV/Excel/PDF/Print, column visibility toggle, responsive
  collapse, full paging/length/search/info. Every list page in the app inherits this automatically.
- **Bulk import** (CSV, no external dependency — see `Services/CsvUtility.cs`) is available on:
  Products (3 types — see below), Product Categories, Locations, and Users. Each has a
  "Download CSV Template" link in its import modal. Exports are handled by the DataTables Buttons
  toolbar on every list (no separate export endpoints needed).
- **Approval / rejection workflow** for Internal Transfer, Item Adjustment Note, and Consumption
  Voucher: every document is created as **Pending** and does **not** touch the stock ledger until
  an Admin/Manager/Supervisor approves it (`Approve` posts to `IStockService`; `Reject` requires a
  reason). Only Pending documents can be edited away (deleted) — Approved/Rejected are immutable
  for audit purposes. Status badges and Approve/Reject buttons appear on both the list and detail
  pages.
- **Products page toolbar**: New Product, an Import dropdown (Import Product / Import Opening
  Stock / Import Customer-wise Product Name), a "Duplicate Code Filter" toggle (flags products
  sharing the same Product Code), and Category/Status/Stock-level filters — all client-side and
  reactive, no page reloads.
- **HS Code** field added to Product (customs/tax classification), shown on the form and in the
  Products grid.
- **CustomerProductAlias** (`Models/Inventory/CustomerProductAlias.cs`) — lets each customer have
  their own name/code for a product; populated via the "Import Customer-wise Product Name" tool.
  `CustomerName` is free text for now; swap it for a real `CustomerId` FK once the Sales module
  adds a Customer entity.

Since this adds new columns/tables (`Status`, `ApprovedById`, `HsCode`, `CustomerProductAliases`,
etc.), run another migration after pulling this update:

```bash
dotnet ef migrations add ApprovalWorkflowAndImportExport
dotnet ef database update
```

## Product dropdown fix, price history, and stock alerts (latest update)

- **Fixed: products not appearing in Internal Transfer / Adjustment Note / Consumption Voucher.**
  The product list for those three forms was embedded as JSON via
  `System.Text.Json.JsonSerializer.Serialize(Model.Products)` directly in the Razor view, which
  defaults to **PascalCase** (`Id`, `Name`, `Sku`). The JS was reading **camelCase**
  (`p.id`, `p.name`, `p.sku`), so every option came back `undefined` and nothing usable rendered.
  Fixed by passing `JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }`
  in all three forms. This was also very likely why the approval workflow looked "missing" —
  nothing could actually be submitted (ProductId always failed required-field validation), so no
  document ever reached Pending to be approved. The workflow itself was already in place (see the
  "Approval / rejection workflow" section above); it should now be fully usable end-to-end.
- **Product price history**: every edit (via the form or CSV import) that changes CostPrice or
  SalePrice now writes a row to `ProductPriceHistories`, attributed to the user who made the
  change. View it via the new clock-icon action on the Products grid (`/Inventory/Products/PriceHistory/{id}`).
- **Low stock alerts**: a bell icon in the top navbar (visible on every page) shows a live count
  of products at or below their reorder level or fully out of stock, fed by
  `GET /Inventory/LowStockAlerts`. Clicking an alert jumps straight to that product's edit page.
  This is in addition to the existing "Low Stock Alerts" panel on the Inventory dashboard and the
  Stock Level filter on the Products grid.

Run another migration after pulling this update (new `ProductPriceHistories` table):

```bash
dotnet ef migrations add PriceHistoryAndAlerts
dotnet ef database update
```

## DataTables icons, transaction form reliability, and import UX (latest update)

- **DataTables toolbar icons**: switched from icon-only buttons to labeled, color-coded buttons
  (green CSV/Excel, red PDF, blue Print, gray Copy/Columns) with tooltips, so each action is
  identifiable at a glance instead of relying on a bare glyph.
- **Fixed: Internal Transfer / Adjustment Note / Consumption Voucher forms not showing anything
  in the first product row.** The row-building script ran synchronously during initial page parse
  and called Select2 with an explicit `dropdownParent`; on the very first auto-added row this
  could hand Select2 a zero-width/not-yet-laid-out element to measure, making that row's dropdown
  effectively invisible (rows added later via "Add Row" were unaffected, since by then the page
  had settled). Fixed by wrapping the script in `DOMContentLoaded`, dropping the unnecessary
  `dropdownParent` option, and deferring the first row's creation one tick via `setTimeout(...,0)`
  so layout has already happened. Also hardened against a null product list.
- **Import modals now auto-close on a fully clean import** (zero row errors) after a brief pause
  so you can read the success summary, then reset and close automatically — no more manually
  dismissing the modal every time. If a row has errors, the modal stays open so you can read and
  fix them; the grid still refreshes in the background either way.

No new migration needed for this round — it's UI/JS only.

## Purchase module

Built on exactly the same pattern as Inventory — DataTables lists, dynamic line-item forms,
Select2 dropdowns, Notyf/SweetAlert2, CSV import/export, and the Pending → Approve/Reject
workflow — with real interlinking end to end:

**Vendor setup**
- **Vendors** (`/Purchase/Vendors`) — auto-generated VendorCode, category, contact/tax/payment
  terms, CSV import.
- **Vendor Categories** (`/Purchase/VendorCategories`) — simple lookup CRUD, seeded with four
  defaults on first run.

**Procure-to-pay documents** (all approval-gated; each links forward to the next stage)
- **Purchase Order** (`/Purchase/Orders`) — Vendor + product lines with quantity/price.
- **Inward Gate Pass** (`/Purchase/GatePasses`) — vehicle/driver details, optional PO link,
  declared quantities (pre-inspection).
- **Goods Receipt Note** (`/Purchase/GRN`) — optional PO + Gate Pass links, a receiving Location,
  and received qty/cost per line. **Approving a GRN posts directly to the same Inventory stock
  ledger** (`StockTransaction`, type `PurchaseReceipt`) that Internal Transfers/Adjustments/
  Consumption use — so stock-on-hand is always consistent regardless of which module moved it.
- **Purchase Invoice** (`/Purchase/Invoices`) — optional PO + GRN links, product lines with tax,
  computed total.
- **Purchase Invoice Payment** (`/Purchase/Payments`) — pick a vendor to filter invoices down to
  ones with an outstanding balance; pay by Cash/Bank/Cheque, optionally drawing down a vendor
  advance (approving the payment decrements that advance's available balance).
- **Debit Note** (`/Purchase/DebitNotes`) — claim/return against a vendor; optionally linked to an
  Invoice/GRN. If a return Location is set, approving it **reverses stock** in the same ledger
  (type `PurchaseReturn`).

**Cash/banking**
- **Issued Cheque Management** (`/Purchase/Cheques`) — approval marks a cheque as officially
  issued; separately, Admin/Manager/Accountant can then mark an issued cheque Cleared or Bounced
  at the bank via dedicated grid actions.
- **Advances** (`/Purchase/Advances`) — vendor advance payments (optionally cheque-funded);
  tracks `AdjustedAmount`/`AvailableAmount` as invoice payments draw against it.

Approver roles differ slightly by document to match real-world sign-off: Admin/Manager/Accountant
approve financial documents (PO, Invoice, Payment, Debit Note, Cheque, Advance); Admin/Manager/
Store Incharge approve warehouse-facing ones (Gate Pass, GRN). All six roles can *submit* (Store
Incharge/Supervisor included) — only the roles above can Approve/Reject.

Run one more migration for this module:

```bash
dotnet ef migrations add PurchaseModule
dotnet ef database update
```

## Cascading auto-populate + GRN inspection quantities (latest update)

The Purchase document chain now pre-fills forward, the way it would in a real procurement flow:

- **Purchase Order → Inward Gate Pass**: selecting a PO on the Gate Pass form fetches
  `GET /Purchase/Orders/GetDetails/{id}` and auto-fills the vendor plus one product line per PO
  line (declared quantity = PO quantity). Everything stays editable afterward.
- **Inward Gate Pass → Goods Receipt Note**: selecting a Gate Pass on the GRN form fetches
  `GET /Purchase/GatePasses/GetDetails/{id}` and fills vendor, the linked PO (if any), and one
  line per gate pass item. A GRN can also be filled directly from a PO (skipping the gate pass)
  via the same `Purchase/Orders/GetDetails` endpoint, which additionally seeds Unit Cost from the
  PO's unit price.
- **Goods Receipt Note → Purchase Invoice**: selecting a GRN on the Invoice form fetches
  `GET /Purchase/GRN/GetDetails/{id}` and fills vendor, the linked PO, and one line per GRN item
  — using **accepted** quantity and unit cost (rejected quantities are never billed). An invoice
  can also be filled directly from a PO the same way.

**GRN inspection — Accepted / Rejected quantities.** Each GRN line now has three quantity
columns: **Declared** (expected, copied from the source PO/Gate Pass), **Accepted** (editable —
defaults to Declared), and **Rejected**, which is **live-calculated client-side** as
`max(0, Declared − Accepted)` on every keystroke in either the Declared or Accepted field — no
manual entry needed, and the row highlights if Accepted exceeds Declared. Only the **Accepted**
quantity posts to the stock ledger on approval and is billed on the line total
(`AcceptedQuantity × UnitCost`); rejected quantities are recorded for the audit trail but never
touch stock. This required a schema change (`GoodsReceiptNoteItem.ReceivedQuantity` →
`DeclaredQuantity` / `AcceptedQuantity` / `RejectedQuantity`), so run one more migration:

```bash
dotnet ef migrations add GrnInspectionAndAutoPopulate
dotnet ef database update
```

The same `GetDetails` pattern is easy to extend to Debit Notes (pre-fill from an Invoice/GRN) if
you want that too — it's the same three-line shape: a controller `GetDetails/{id}` JSON action,
a `<select>` change handler that fetches it, and a `clearRows()` + `addItemRow(prefill)` pair
already present in every transactional form's script block.

## Print/export, tax fields, and live calculation (latest update)

- **Auto-generated numbers**: already true everywhere — every document (PO, Gate Pass, GRN,
  Invoice, Payment, Debit Note, Cheque, Advance, Vendor, Product) gets a system-generated number
  on save; no form has an editable number field.
- **Vendor tax fields**: `NTNNumber` (format `0000000-0`) and `SalesTaxRegistrationNumber` (format
  `00-00-0000-000-00`) on every vendor, validated both client-side (input mask) and server-side
  (regex). The same fields exist on `CompanySettings` (`/Company`) so your own NTN/STRN print on
  every document too.
- **Print / Export PDF / Export Excel — every document type.** A single shared, reusable
  layout (`Views/Shared/_PrintDocument.cshtml` + `PrintDocumentViewModel`) renders a professional
  invoice-style sheet — company letterhead, document info box, two-column party details (with
  NTN/STRN), item table, totals box, amount-in-words, bank details, and signature lines — matching
  the sample invoice layout. Every one of the 11 voucher/document controllers (PO, Gate Pass, GRN,
  Invoice, Payment, Debit Note, Cheque, Advance, Internal Transfer, Adjustment Note, Consumption
  Voucher) has a `Print(id)` action building this from its own data, reached via a print icon on
  every list/detail page. The print page itself has three buttons: **Print** (browser print
  dialog — "Save as PDF" works there natively too), **PDF** (client-side `html2canvas` + `jsPDF`,
  no server dependency), and **Excel** (client-side `SheetJS`, exports the item table).
- **Sales Tax dropdown (16/17/18/19%) with live calculation** on every line item across Purchase
  Order, GRN, Purchase Invoice, and Debit Note — selecting a rate instantly recalculates that
  row's amount and the document's running total, no page reload.
- **Withholding Income Tax** (0%–5.5% in 0.5% steps) as a header-level field on Purchase Invoice
  — calculated on the pre-GST sub-total, shown as a deduction, with a live **Net Payable** total
  that's what actually carries through to the Invoice Payment module's balance calculations
  (fixed a real bug here: the payment module was computing "balance owed" from the gross total
  instead of net-of-withholding — now consistent everywhere an invoice balance is shown).
- Amounts in words on printouts use a small `NumberToWordsConverter` utility (English long-scale,
  e.g. "Three Hundred and Ten Thousand Five Hundred Only") — no external dependency.

New tables/columns in this round need one more migration:

```bash
dotnet ef migrations add PrintAndTaxFields
dotnet ef database update
```

**Known limitation:** the sample invoice includes an FBR-specific QR code and "FBR Digital
Invoicing" badge tied to Pakistan's live e-invoicing integration — that requires a real FBR API
account and is out of scope here without one. Everything else in the layout is reproduced.

## Purchase Invoice Payment tweak + Sales module (latest update)

- **Tweak**: on the Purchase Invoice Payment form, selecting an invoice now auto-populates the
  vendor and the payment amount (with that invoice's outstanding balance) — works whichever
  field you fill in first, vendor or invoice.

- **Sales module**, built on the exact same pattern as Purchase — approval-gated documents,
  DataTables lists, dynamic line items with live tax calculation, cascading auto-populate, CSV
  import for bulk entities, and every document printable/exportable via the shared print layout.

  **Lookups**
  - **Customer Categories** / **Customers** (`/Sales/CustomerCategories`, `/Sales/Customers`) —
    same CRUD + CSV import pattern as Vendor Categories/Vendors. Customer code auto-generated
    (`CUS-000001`). Customers carry NTN and STRN fields too, with the same input masks and
    validation as vendors.

  **Documents** (Pending → Approve/Reject, immutable once decided, deletable only while Pending)
  - **Sale Order** (`/Sales/Orders`) — customer + line items with Sales Tax dropdown and live
    line/grand-total calculation.
  - **Delivery Challan** (`/Sales/DeliveryChallans`) — optional link to a Sale Order (auto-fills
    customer + product lines on selection); a shipping Location; **approval posts a
    `SalesDelivery` movement to the same stock ledger Inventory and Purchase both use** — so
    shipping goods out reduces stock exactly like a GRN increases it.
  - **Sales Tax Invoice** (`/Sales/Invoices`) — the module's namesake document, matching the
    sample invoice layout exactly on print. Optional link to a Sale Order or Delivery Challan
    (either auto-fills customer + lines); Sales Tax dropdown per line with live Sub
    Total/Tax/Total Net Amount calculation.
  - **Sales Invoice Payment** (`/Sales/Payments`) — pick a customer *or* an invoice first, either
    order: selecting a customer narrows the invoice list to their unpaid invoices; selecting an
    invoice auto-fills the customer and the outstanding balance as the payment amount (same UX as
    the Purchase side).
  - **Credit Note** (`/Sales/CreditNotes`) — claim/return in the customer's favor; optional link
    to an invoice/challan. If a return Location is set, approval posts a `SalesReturn` (positive)
    stock movement — leave it blank for a pure financial credit with no stock impact.

  Approver roles mirror the Purchase side: Admin/Manager/Accountant approve financial documents,
  Admin/Manager/Store Incharge approve the Delivery Challan (since it's a warehouse action).
  Submission is open to a broader set of roles (adds Standard User) since capturing a sale and
  approving it are meant to be different people.

New tables need one more migration:

```bash
dotnet ef migrations add SalesModule
dotnet ef database update
```

## 16 UX/data tweaks (latest update)

- **Layout**: horizontal page scrollbar removed app-wide (wide tables/toolbars scroll within
  themselves instead); Products page filters forced onto one row (scrolls horizontally within the
  toolbar on narrow screens rather than wrapping to a second line).
- **Payment Terms** on Vendor and Customer is now a fixed dropdown (On Receipt / 7 / 15 / 30 / 45
  / 60 / 75 days) instead of a free-number input.
- **Phone numbers** (`.phone-input`, used on Vendor/Customer/User/Profile forms) now validate live
  as you type via intl-tel-input's `isValidNumber()`, with inline valid/invalid feedback text.
- **Country → City cascading dropdown** on Vendor and Customer: a curated list of ~20 countries
  and their major cities, built with Select2 `tags: true` — pick from the list or just type a
  city that isn't there.
- **Voucher numbering** changed app-wide to `{PREFIX}-{yyyyMM}-{0001}` (e.g. `PO-202609-0001`),
  resetting every calendar month. One change to `IStockService.GenerateDocumentNumberAsync`
  affects all 13 document types automatically. Product SKU / Vendor Code / Customer Code are
  unaffected — those stay permanent sequential identifiers, not date-scoped.
- **Print system**: the shared print layout now puts Our Information / Party / Document Details
  in a single 3-column row; adds a live "Columns" checklist to show/hide table columns before
  printing or exporting; and adds drag-to-resize column borders — all three land on every one of
  the 11 printable documents in one shared-template change. Vendor Code / Customer Code now also
  print in the party block everywhere.
- **Company logo uploads** now accept SVG (scoped to the logo only — product/avatar uploads are
  unchanged) since an `<img>`-rendered SVG doesn't execute embedded scripts.
- **Product auto-populate**: selecting a product in a line item now auto-fills Unit Price from
  the product's cost price (Purchase-side: PO, GRN, Purchase Invoice, Debit Note) or sale price
  (Sales-side: Sale Order, Sales Invoice, Credit Note) if the price field is still empty.
- **Standard packing quantity**: Products can carry a `PackingUnitName` (e.g. "Box") and
  `PackingQuantity` (e.g. 24). Every document with a quantity field — Purchase Order, GRN,
  Purchase Invoice, Debit Note, Sale Order, Sales Invoice, Credit Note, Inward Gate Pass, and
  Delivery Challan — shows a live "≈ X Box(es)" hint under the quantity input, recalculated as
  you type.
- **Four product types** — Saleable, Purchasable, Service, Consumable — as a `[Flags]` enum on
  Product (a product can be more than one). Purchase-side product dropdowns now only show
  Purchasable + Consumable products; Sales-side dropdowns only show Saleable + Service products.
  Inventory's internal documents (Transfer/Adjustment/Consumption) are unfiltered by design, since
  they're warehouse operations, not sales/purchase transactions.
- **Customer PO Number**: Sale Order and Sales Invoice now carry the customer's own PO reference
  (mirrors the existing `VendorInvoiceNumber` field pattern), shown on the form and on print.
- **Cascading auto-populate — now complete everywhere**: the last two gaps are closed. Debit Note
  now auto-fills vendor + product lines from a selected Purchase Invoice or GRN; Credit Note now
  auto-fills customer + product lines from a selected Sales Invoice or Delivery Challan. This
  required one new `GetDetails` endpoint (`PurchaseInvoicesController`) since Sales Invoice/GRN/
  Delivery Challan already had theirs from earlier work.
- **Purchase Invoice Payment tweak**: selecting an invoice now also auto-populates the vendor and
  the payment amount (outstanding balance) — same UX already in place on the Sales side.
- Confirmed already satisfied, no change needed: Products are searchable by both code and name
  (DataTables' global search already checks every column, including the hidden Product Code
  column) — a combined display column wasn't necessary.

New/changed columns in this round need one more migration:

```bash
dotnet ef migrations add TweaksRound1
dotnet ef database update
```

**Deferred by explicit choice**: removing AdminLTE for plain Bootstrap + custom styling is a full
shell rebuild (sidebar, navbar, treeview, card system) and was intentionally left for a separate
follow-up effort rather than folded in here.

## Vendor-from-customer code, multi-PO, product-code search, price propagation, payment WHT, real-time dashboards (latest update)

- **Vendor code assigned by our customers**: `Customer.OurVendorCodeAtCustomer` — the code *the
  customer* uses to identify our company as their vendor (distinct from our own internal
  `CustomerCode`). Shown on the Customer form and printed in the party block of every Sales
  document.
- **Multiple Customer PO numbers**: `CustomerPONumber` (singular) is now `CustomerPONumbers` on
  both Sale Order and Sales Invoice, backed by a reusable tags-style multi-entry field
  (`initTagsToHiddenField` in site.js) — type a PO number, press Enter, repeat for as many as the
  customer issued. Stored as a comma-separated string.
- **Product search now uses Product Code, not SKU**: `ProductCode` was already a field distinct
  from `Sku` on Product; every product-select dropdown across all 12 transactional forms now
  displays and searches by Product Code (falling back to SKU only if a product has none set).
  Also fixed a real bug in the process: every printed document's "Product Code" column was
  actually bound to `Sku` — corrected across all 12 controllers.
- **Full price propagation chain, closed end-to-end**: Delivery Challan and Inward Gate Pass
  previously had no price field at all, so selecting one to auto-populate a downstream document
  (Sales Invoice from a Challan, GRN from a Gate Pass) left the price blank. Both now carry
  `UnitPrice`/`UnitCost` — populated from the linked Sale Order/PO when one exists, from the
  product's own price otherwise, and always editable — so the full chain (Product → SO/PO →
  Challan/Gate Pass → Invoice/GRN) now carries real price data all the way through.
- **Payment Terms + auto Due Date on Sales Invoice**: a Payment Terms dropdown (On Receipt / 7 /
  15 / 30 / 45 / 60 / 75 days) live-calculates Due Date from Invoice Date; selecting a customer
  defaults the terms to that customer's own standard terms (still fully overridable).
- **Withholding tax on both payment documents**: Purchase Invoice Payment already had the
  model/calculation logic from earlier work — only the form UI and print display were missing,
  now added. Sales Invoice Payment got the full treatment (model, view model, controller, form,
  print) since a customer can withhold tax when paying us too. Both show a live "Net
  Payment/Net Received" figure as you type — the invoice balance is still reduced by the full
  Amount; this is the actual cash/bank/cheque that changes hands.
- **Real-time dashboards**: the main Dashboard (`/Dashboard`) was previously showing hardcoded
  placeholder numbers ("128", "PKR 1.2M", etc.) left over from the original shell — it's now
  fully rebuilt on live database queries (sale orders this month, purchase order count, products
  with stock on hand, pending approvals across every module). All four dashboards (main, Sales,
  Purchases, Inventory) now have a `GetSummary` JSON endpoint and poll it every 20 seconds,
  updating the small-box numbers in place with no page reload. Caught and fixed a second instance
  of the earlier withholding-tax bug while rebuilding these — the Purchases dashboard's
  Outstanding Payables figure was using the gross `TotalAmount` instead of `NetPayableAmount`.
- **Column selection on print, confirmed**: the shared print template's "Columns" checklist
  (built in the previous round) already includes every column present in each document, on all
  11 printable documents — nothing further was needed here.

New/changed columns in this round need one more migration:

```bash
dotnet ef migrations add TweaksRound2
dotnet ef database update
```

## Accounts module (latest update)

A full general-ledger accounting layer, built on the same patterns as Purchase/Sales/Inventory —
approval-gated vouchers, DataTables lists, live calculations, print/export — plus a real
double-entry posting engine underneath it.

**Foundation**
- **Chart of Accounts** (`/Accounts/ChartOfAccounts`) — hierarchical accounts (Asset/Liability/
  Equity/Income/Expense), each with a code, optional parent, opening balance, and a live current
  balance computed from posted journal entries. A starter chart is seeded on first run (Cash,
  Bank, AR, AP, Inventory, Sales Revenue, COGS, common expense accounts). The handful of accounts
  the app posts to automatically are flagged `IsSystemAccount` and protected from deletion.
- **Employees** (`/Accounts/Employees`) — a minimal employee master (code, department,
  designation, salary) added because no Payroll module exists yet, needed so vouchers can be
  tagged to an employee for their ledger.
- **`IAccountingService`** — the shared GL engine. Every voucher type below posts a balanced
  `JournalEntry` (with `JournalEntryLine` rows) here on approval; it validates debits equal
  credits and throws if not, as a last line of defense.
- **`ILedgerService`** — derives a Customer/Vendor/Employee's transaction history directly from
  the existing Sales/Purchase documents (invoices, payments, credit/debit notes) plus any journal
  lines explicitly tagged to that party, with a running balance. This is what Ledgers, Account
  Statements, and Invoices & Recoveries are all built on.

**The six voucher types** (all Pending → Approve/Reject → posts to the GL, immutable once
decided, deletable only while Pending)
- **Journal Voucher** (`/Accounts/JournalVouchers`) — fully flexible multi-line double-entry, with
  a live debit/credit balance indicator that won't let you submit an unbalanced entry. Each line
  can optionally tag a Customer/Vendor/Employee for their ledger.
- **Cash Payment Voucher** / **Cash Receipt Voucher** — single-line vouchers against a chosen Cash
  account, auto-balanced against a contra account (what was paid for / received for). Optional
  party tagging.
- **Bank Payment Voucher** / **Bank Receipt Voucher** — same pattern against a Bank account, with
  Bank Transfer or Cheque as the method; cheque ones capture number/date and feed the unified
  Cheque Register.
- **Inventory Voucher** (`/Accounts/InventoryVouchers`) — books the *accounting* impact of an
  inventory valuation change (write-off, damage, revaluation), separate from Item Adjustment
  Note's *physical* stock-quantity movement — a write-off typically needs both, and this voucher
  can optionally link to the adjustment note that prompted it.

**Reporting & cross-cutting views**
- **All Vouchers** (`/Accounts/AllVouchers`) — every voucher from all six types above in one
  filterable list (by type, by status).
- **Ledgers** (`/Accounts/Ledgers`) — pick a Customer, Vendor, or Employee and see their full
  transaction history with running balance, on screen.
- **Account Statements** (`/Accounts/AccountStatements`) — the same data as Ledgers, but
  date-range-filterable and rendered through the shared print template as a formal statement
  ready to print, export as PDF, or export as Excel.
- **Invoices & Recoveries** (`/Accounts/InvoicesAndRecoveries`) — AR/AP aging: total invoiced,
  total recovered, outstanding balance, and days overdue, across every customer and vendor,
  tabbed by direction.
- **Cheques Management** (`/Accounts/ChequesManagement`) — a single register unifying Issued
  Cheques (Purchase payments, already existed), Received Cheques (added `ClearanceStatus` to
  `SalesInvoicePayment`), and cheque-based Bank Payment/Receipt Vouchers — one shared
  Cleared/Bounced action works across all four sources.
- **Accounts dashboard** (`/Accounts`) — cash & bank position, total receivable/payable, pending
  approvals across all six voucher types, with the same 20-second auto-refresh pattern as the
  other module dashboards.

New tables need one more migration:

```bash
dotnet ef migrations add AccountsModule
dotnet ef database update
```

**Scope note on Customer/Vendor ledgers**: rather than retroactively posting a journal entry for
every historical Sales/Purchase document (which would mean touching the Approve() action on ~13
existing controllers), `ILedgerService` derives the ledger directly from those documents' existing
Amount/Date/party fields — functionally equivalent to a posted ledger, without the large surface
area of a retrofit. Any new Customer/Vendor activity recorded via a Journal/Cash/Bank voucher
(tagged to that party) shows up in the same ledger seamlessly, so both sources blend into one
chronological view.

## Where to plug in the rest of the ERP

## ZKTeco attendance push

The app exposes a ZKTeco ADMS-compatible push receiver at `/api/zkteco/iclock/cdata` and the no-command poll route at `/api/zkteco/iclock/getrequest`. Configure an active biometric device in Payroll Settings with the exact device serial number, then set a shared API key in the Machine Integration section. Configure the terminal's server address to this app's reachable URL and send the shared key as `KEY`, `X-Api-Key`, or a bearer token. The device PIN must match an active employee's employee code or numeric employee ID.

The receiver responds to device option handshakes, accepts tab-delimited `ATTLOG` punches, and merges the earliest/latest punches for an employee/date into the unique daily attendance record. `ZKTeco-Push.http` contains handshake, poll, and sample attendance requests. The device must be able to reach the app over the network; do not expose the endpoint publicly without a strong shared key and network-level access restrictions.

Each of these controllers currently just renders a "coming soon" placeholder view — build
the real functionality directly into them (or replace them) as you develop each module with
Codex. (Inventory and Purchase are already fully built — see the sections above.)

```
Controllers/SalesController.cs
Controllers/AccountsController.cs
Controllers/PayrollController.cs
Controllers/ReportsController.cs
Controllers/ChartOfAccountsController.cs
Controllers/CompanyController.cs
Controllers/SettingsController.cs
```

Add new `DbSet<T>` entities to `Data/ApplicationDbContext.cs` as each module needs its own
tables, then `dotnet ef migrations add <ModuleName>` + `dotnet ef database update`.

## Project structure

```
ERP.Identity/
├── Controllers/          # Account, Dashboard, Profile, Users + module placeholders
├── Data/                 # ApplicationDbContext, DbInitializer (roles + admin seed)
├── Models/
│   ├── ApplicationUser.cs
│   ├── AppRoles.cs
│   └── ViewModels/
├── Services/              # IFileUploadService (avatar upload)
├── Views/
│   ├── Account/           # Login, AccessDenied
│   ├── Dashboard/
│   ├── Profile/            # My Account, Change Password
│   ├── Users/               # Admin user/role management (DataTables)
│   ├── Shared/               # _Layout (AdminLTE), _LoginLayout, _AvatarUpload partial
│   └── Sales|Purchases|Inventory|Accounts|Payroll|Reports|ChartOfAccounts|Company|Settings/
└── wwwroot/
    ├── css/styles.css         # default application styling
    ├── js/site.js          # Notyf, SweetAlert2, avatar preview, select2/intl-tel-input/flatpickr/DataTables init
    ├── images/default-avatar.svg
    └── uploads/avatars/    # uploaded profile photos land here
```

## Swapping SQLite for SQL Server (optional)

In `Program.cs` replace:

```csharp
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
```

with:

```csharp
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
```

add the `Microsoft.EntityFrameworkCore.SqlServer` package, and update the connection string
in `appsettings.json`.
