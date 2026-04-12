using System.Net;
using System.Net.Mail;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infra.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public SmtpEmailService(IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings");
            _smtpServer = emailSettings["SmtpServer"]!;
            _smtpPort = int.Parse(emailSettings["SmtpPort"]!);
            _senderEmail = emailSettings["SenderEmail"]!;
            _senderPassword = emailSettings["SenderPassword"]!;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(recipientEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
