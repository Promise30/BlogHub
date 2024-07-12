using BloggingAPI.Constants;

namespace BloggingAPI.Infrastructure.Services.Interface
{
    public interface IEmailService
    {
        Task<string> SendEmail(EmailMessage message);   
    }
}
