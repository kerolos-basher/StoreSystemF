
namespace Application.Services.Email;

public interface IEmailSender
{
    Task<bool> SendEmailAsync(EmailMessage message, bool IsAttachment = false);

}
