using DuAnTotNghiep.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace DuAnTotNghiep.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var senderEmail = _configuration["GmailSettings:SenderEmail"];
            var senderName = _configuration["GmailSettings:SenderName"];
            var appPassword = _configuration["GmailSettings:AppPassword"];

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(appPassword))
            {
                // Skip sending when SMTP credentials are not configured.
                return;
            }

            var fromAddress = new MailAddress(senderEmail, senderName);
            var toAddress = new MailAddress(to);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, appPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}
