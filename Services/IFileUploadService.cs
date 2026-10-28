using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ERP.Identity.Services
{
    public interface IFileUploadService
    {
        /// <summary>Saves the file under wwwroot/uploads/avatars and returns the web-relative path.</summary>
        Task<string> SaveAvatarAsync(IFormFile file, string userId);

        /// <summary>Saves the file under wwwroot/uploads/products and returns the web-relative path.</summary>
        Task<string> SaveProductImageAsync(IFormFile file, string productKey);

        /// <summary>Saves the file under wwwroot/uploads/company and returns the web-relative path.</summary>
        Task<string> SaveCompanyLogoAsync(IFormFile file);

        /// <summary>Saves an expense receipt under wwwroot/uploads/receipts and returns the web-relative path.</summary>
        Task<string> SaveReceiptAsync(IFormFile file, string receiptKey);

        void DeleteIfExists(string? relativePath);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxSizeBytes = 3 * 1024 * 1024; // 3 MB

        public FileUploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public Task<string> SaveAvatarAsync(IFormFile file, string userId) => SaveImageAsync(file, "avatars", userId);

        public Task<string> SaveProductImageAsync(IFormFile file, string productKey) => SaveImageAsync(file, "products", productKey);

        public Task<string> SaveCompanyLogoAsync(IFormFile file) => SaveImageAsync(file, "company", "logo", allowSvg: true);

        public Task<string> SaveReceiptAsync(IFormFile file, string receiptKey) => SaveImageAsync(file, "receipts", receiptKey);

        private async Task<string> SaveImageAsync(IFormFile file, string subFolder, string key, bool allowSvg = false)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = allowSvg ? AllowedExtensions.Append(".svg").ToArray() : AllowedExtensions;

            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException(allowSvg
                    ? "Only JPG, PNG, GIF, WEBP or SVG images are allowed."
                    : "Only JPG, PNG, GIF or WEBP images are allowed.");

            if (file.Length > MaxSizeBytes)
                throw new InvalidOperationException("Image must be smaller than 3 MB.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{key}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{subFolder}/{fileName}";
        }

        public void DeleteIfExists(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath))
            {
                try { File.Delete(fullPath); } catch { /* ignore cleanup failures */ }
            }
        }
    }
}
