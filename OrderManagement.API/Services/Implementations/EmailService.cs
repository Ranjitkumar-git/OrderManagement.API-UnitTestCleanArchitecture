using Microsoft.Extensions.Options;
using OrderManagement.API.Configurations;
using System.Net;
using System.Net.Mail;

namespace OrderManagement.API.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailOptions)
        {
            _emailSettings = emailOptions.Value;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(
                    _emailSettings.FromEmail,
                    _emailSettings.FromName),

                Subject = subject,

                Body = body,

                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var smtp = new SmtpClient(
                _emailSettings.Host,
                _emailSettings.Port);

            smtp.Credentials = new NetworkCredential(
                _emailSettings.Username,
                _emailSettings.Password);

            smtp.EnableSsl = _emailSettings.EnableSsl;

            await smtp.SendMailAsync(message);
        }
    }
}