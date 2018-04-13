using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AutoServiss.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(List<MailAddress> mailTo, string subject, string body);
    }
}