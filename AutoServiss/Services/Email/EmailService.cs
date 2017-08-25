using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
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

        public async Task<Response> SendEmailAsync(List<EmailAddress> mailTo, string subject, string body)
        {
            var emailMessage = new SendGridMessage();

            emailMessage.SetFrom(new EmailAddress(_settings.FromEmail));
            emailMessage.AddTos(mailTo);
            emailMessage.SetSubject(subject);
            emailMessage.AddContent(MimeType.Html, body);

            var client = new SendGridClient(_settings.SendGridApiKey);
            return await client.SendEmailAsync(emailMessage);          
        }
    }
}
