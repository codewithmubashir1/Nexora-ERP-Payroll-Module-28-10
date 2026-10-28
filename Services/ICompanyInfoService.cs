using ERP.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Services
{
    public interface ICompanyInfoService
    {
        Task<Models.CompanySettings> GetAsync();
    }

    public class CompanyInfoService : ICompanyInfoService
    {
        private readonly ApplicationDbContext _db;

        public CompanyInfoService(ApplicationDbContext db) => _db = db;

        public async Task<Models.CompanySettings> GetAsync()
        {
            var settings = await _db.CompanySettings.FirstOrDefaultAsync();
            if (settings != null) return settings;

            settings = new Models.CompanySettings();
            _db.CompanySettings.Add(settings);
            await _db.SaveChangesAsync();
            return settings;
        }
    }
}
