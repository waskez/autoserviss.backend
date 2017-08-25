using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoServiss.Services.Email
{
    public interface IEmailService
    {
        Task<Response> SendEmailAsync(List<EmailAddress> mailTo, string subject, string body);
    }
}
