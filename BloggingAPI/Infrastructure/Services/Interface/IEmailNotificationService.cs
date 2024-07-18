using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Infrastructure.Services.Interface
{
    public interface IEmailNotificationService
    {
        Task SendEmailConfirmationLinkAsync(string url, string email);
        Task SendNewCommentNotificationAsync(int postId, Comment newComment);
        Task SendNewPostNotificationAsync(Post newPost);
        Task SendResetPasswordPasswordEmailAsync(string email, string url);
    }
}