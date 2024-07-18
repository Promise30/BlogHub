using BloggingAPI.Services.Constants;

namespace BloggingAPI.Services.Interface
{
    public interface IEmailService
    {
        Task<string> SendEmail(EmailMessage message);
    }
}
