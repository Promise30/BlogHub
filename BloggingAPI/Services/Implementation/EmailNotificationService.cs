
using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;
using BloggingAPI.Services.Constants;
using BloggingAPI.Services.Interface;
using CloudinaryDotNet;

namespace BloggingAPI.Services.Implementation
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly IRepositoryManager _repositoryManager;
        private readonly string baseUrl = "https://localhost:7156/api/blogs/posts";
        public EmailNotificationService(IEmailService emailService, IHttpContextAccessor httpContextAccessor, ILogger<EmailNotificationService> logger, IRepositoryManager repositoryManager)
        {
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _repositoryManager = repositoryManager;
        }
        public async Task SendEmailConfirmationLinkAsync(string url, string email)
        {
            try
            {
                var emailBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <body>
                        <p>Thank you for registering to use our service</p>
                        <p>Kindly, click on the link below to confirm your email registration</p>
                        <p>{url}</p>
                        <p>Best regards,<br/>Blogging Platform Service.</p>
                    </body>
                    </html>";
                var message = new EmailMessage(new string[] { email }, "Email Confirmation", emailBody);

                var emailResponse = await _emailService.SendEmail(message);
                _logger.Log(LogLevel.Information, $"Response from sending mail: {emailResponse}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occurred while sending new post notification email: {ex.Message}");
            }

        }
        public async Task SendResetPasswordPasswordEmailAsync(string url, string email)
        {
            try
            {
                var emailBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <body>
                        <p>Your requested to change your password</p>
                        <p>Kindly, click on the link below to reset your password</p>
                        <p>{url}</p>
                        <p>If you do not request to change your password, kindly disregard this mail</p>
                        <p>Best regards,<br/>Blogging Platform Service.</p>
                    </body>
                    </html>";
                var message = new EmailMessage(new string[] { email }, "Password Reset Link", emailBody);

                var emailResponse = await _emailService.SendEmail(message);
                _logger.Log(LogLevel.Information, $"Response from sending mail: {emailResponse}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occurred while sending new post notification email: {ex.Message}");
            }

        }
        public async Task SendNewPostNotificationAsync(Post newPost)
        {
            try
            {
                var post = await _repositoryManager.Post.GetPostwithUserAsync(newPost.Id);
                var authorEmail = post.User.Email;
                //var postLink = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/blogs/posts/{newPost.Id}";
                var postLink = baseUrl + $"/{newPost.Id}";

                var emailBody = $@"
                <!DOCTYPE html>
                <html>
                <body>
                    <p>Hello, {newPost.Author},</p>
                    <p>We're excited to inform you that your new post titled <b>{newPost.Title}</b> has been successfully created.</p>
                    <p>You can view your new post by following this link: <a href='{postLink}'>{newPost.Title}</a></p>
                    <p>Thank you for sharing your content with us!</p>
                    <br/><br/>
                    <p>Best regards,<br/>Blogging Platform Service.</p>
                </body>
                </html>
            ";

                var message = new EmailMessage(new string[] { authorEmail }, "New Post Created", emailBody);
                _logger.Log(LogLevel.Information, $"Email content will be sent to {newPost.Author}.");

                var emailResponse = await _emailService.SendEmail(message);
                _logger.Log(LogLevel.Information, $"Response from sending mail: {emailResponse}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occurred while sending new post notification email: {ex.Message}");
            }
        }

        public async Task SendNewCommentNotificationAsync(int postId, Comment newComment)
        {
            try
            {
                var post = await _repositoryManager.Post.GetPostwithUserAsync(postId);
                var authorEmail = post.User.Email;
                //var postLink = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/api/blogs/posts/{post.Id}/comments/{newComment.Id}";
                var postLink = baseUrl + $"/{postId}/comments/{newComment.Id}";

                var emailBody = $@"
                <!DOCTYPE html>
                <html>
                <body>
                    <p>Hello, {post.Author},</p>
                    <p>We're excited to inform you that your post titled <b>{post.Title}</b> has just received a new comment.</p>
                    <p>You can view your new comment by following this link: <a href='{postLink}'>{post.Title}</a></p>
                    <p>Thank you for sharing your content with us!</p>
                    <br/><br/>
                    <p>Best regards,<br/>Blogging Platform Service.</p>
                </body>
                </html>
            ";

                var message = new EmailMessage(new string[] { authorEmail }, "New Post Comment", emailBody);
                _logger.Log(LogLevel.Information, $"Email content will be sent to {post.Author}.");

                var emailResponse = await _emailService.SendEmail(message);
                _logger.Log(LogLevel.Information, $"Response from sending mail: {emailResponse}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occurred while sending new post notification email: {ex.Message}");
            }
        }
    }
}
