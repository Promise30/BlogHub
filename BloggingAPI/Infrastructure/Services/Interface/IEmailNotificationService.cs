using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Infrastructure.Services.Interface
{
    public interface IEmailNotificationService
    {
        Task SendNewCommentNotificationAsync(int postId, Comment newComment);
        Task SendNewPostNotificationAsync(Post newPost);
    }
}