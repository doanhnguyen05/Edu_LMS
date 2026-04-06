using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EduLMS.Web.Services
{
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpConfig = _configuration.GetSection("Smtp");
            var host = smtpConfig["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(smtpConfig["Port"] ?? "587");
            var username = smtpConfig["Username"] ?? "";
            var password = smtpConfig["Password"] ?? "";
            var fromName = smtpConfig["FromName"] ?? "EduLMS";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, username));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation("Email sent to {Email} with subject: {Subject}", email, subject);
        }
    }
}
