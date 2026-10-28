using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ERP.Identity.Services
{
    public sealed class DocumentEmailOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "ERP System";
        public bool EnableSsl { get; set; } = true;
    }

    public interface IDocumentEmailSender
    {
        bool IsConfigured { get; }
        Task SendPdfAsync(string recipient, string subject, string body, string fileName, Stream pdf);
    }

    public sealed class SmtpDocumentEmailSender : IDocumentEmailSender
    {
        private readonly DocumentEmailOptions _options;

        public SmtpDocumentEmailSender(IOptions<DocumentEmailOptions> options) => _options = options.Value;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.Host) &&
            !string.IsNullOrWhiteSpace(_options.FromAddress) && _options.Port is > 0 and <= 65535;

        public async Task SendPdfAsync(string recipient, string subject, string body, string fileName, Stream pdf)
        {
            if (!IsConfigured) throw new InvalidOperationException("Document email is not configured. Set DocumentEmail SMTP settings first.");

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(new MailAddress(recipient));
            message.Attachments.Add(new Attachment(pdf, fileName, "application/pdf"));

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            if (!string.IsNullOrWhiteSpace(_options.UserName))
                client.Credentials = new NetworkCredential(_options.UserName, _options.Password);

            await client.SendMailAsync(message);
        }
    }
}