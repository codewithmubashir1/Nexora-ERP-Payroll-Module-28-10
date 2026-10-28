using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var pakistanCulture = CultureInfo.GetCultureInfo("en-PK");

// ---------- Services ----------
builder.Services.AddControllersWithViews();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(pakistanCulture);
    options.SupportedCultures = new[] { pakistanCulture };
    options.SupportedUICultures = new[] { pakistanCulture };
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password policy
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;

        // Lockout
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.Name = "ERP.Identity.Auth";
});

builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ICompanyInfoService, CompanyInfoService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.Configure<DocumentEmailOptions>(builder.Configuration.GetSection("DocumentEmail"));
builder.Services.AddScoped<IDocumentEmailSender, SmtpDocumentEmailSender>();

// The JS in site.js sends the antiforgery token as a "RequestVerificationToken" header
// for every fetch()-based POST (delete, approve/reject, import, toggle-active, etc.).
// Without this, [ValidateAntiForgeryToken] only looks at form fields and rejects those requests.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

var app = builder.Build();

// ---------- Seed database (roles + default admin) ----------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await DbInitializer.SeedAsync(services, app.Configuration);
}

// ---------- Middleware pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Login is the default landing page; authenticated users get redirected
// to the Dashboard from inside AccountController.Login().
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
