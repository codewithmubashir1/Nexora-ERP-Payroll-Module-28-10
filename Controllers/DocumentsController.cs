using System.Net.Mail;
using System.Text;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Documents")]
    public class DocumentsController : Controller
    {
        private const long MaxPdfBytes = 15 * 1024 * 1024;
        private readonly IDocumentEmailSender _emailSender;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentEmailSender emailSender, ILogger<DocumentsController> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpPost("EmailPdf")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxPdfBytes)]
        public async Task<IActionResult> EmailPdf(IFormFile pdf, string recipient, string subject, string message)
        {
            if (!_emailSender.IsConfigured)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "SMTP email is not configured. Configure DocumentEmail settings to send PDF attachments." });
            if (pdf == null || pdf.Length < 5 || pdf.Length > MaxPdfBytes)
                return BadRequest(new { message = "PDF must be between 5 bytes and 15 MB." });
            if (!MailAddress.TryCreate(recipient, out _))
                return BadRequest(new { message = "Enter a valid recipient email address." });

            await using var pdfStream = new MemoryStream();
            await pdf.CopyToAsync(pdfStream, HttpContext.RequestAborted);
            var bytes = pdfStream.ToArray();
            if (!Encoding.ASCII.GetString(bytes, 0, 5).Equals("%PDF-", StringComparison.Ordinal))
                return BadRequest(new { message = "The uploaded attachment is not a valid PDF." });

            var baseName = Path.GetFileNameWithoutExtension(pdf.FileName);
            var safeFileName = string.Concat(baseName.Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '_'));
            if (string.IsNullOrWhiteSpace(safeFileName)) safeFileName = "document";

            try
            {
                pdfStream.Position = 0;
                await _emailSender.SendPdfAsync(recipient, subject ?? "ERP document", message ?? string.Empty,
                    $"{safeFileName}.pdf", pdfStream);
                return Ok(new { success = true, message = $"PDF sent to {recipient}." });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to send a document PDF to {Recipient}.", recipient);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Email delivery failed. Check the SMTP settings and try again." });
            }
        }
    }
}