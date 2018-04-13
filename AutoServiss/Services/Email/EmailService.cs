using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AutoServiss.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _settings;

        public EmailService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(List<MailAddress> mailTo, string subject, string body)
        {
            using (var client = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = _settings.SmtpUsername,
                    Password = _settings.SmtpPassword
                };

                client.Credentials = credential;
                client.Host = _settings.SmtpHost;
                client.Port = _settings.SmtpPort;
                client.EnableSsl = true;

                using (var emailMessage = new MailMessage())
                {
                    foreach (var recipient in mailTo)
                    {
                        emailMessage.To.Add(recipient.Address);
                    }
                    emailMessage.From = new MailAddress(_settings.FromEmail);
                    emailMessage.Subject = subject;
                    emailMessage.Body = body;
                    client.Send(emailMessage);
                }
            }

            await Task.CompletedTask;
        }
    }
}
