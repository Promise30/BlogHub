using BloggingAPI.Services.Interface;

namespace BloggingAPI.Services.Implementation
{
    public class EmailTemplateService : IEmailTemplateService
    {
            public string GenerateEmailChangeConfirmationLink(string userName, string emailChangeConfirmationLink)
            {
                return $"Hello {userName},<br><br>" +
                    $"You requested to change your email. Please confirm the new email by clicking on the link below:<br>" +
                    $"<a href='{emailChangeConfirmationLink}'>Confirm New Email</a><br><br>" +
                    $"If you did not request this, please contact support.";
            }


            public string GeneratePasswordResetEmail(string userName, string passwordResetLink)
            {
                return $"Hello {userName},<br><br>" +
                   $"You requested to reset your password. Please reset your password by using the password reset link below:<br>" +
                   $"<a href='{passwordResetLink}'>Reset Password</a><br><br>" +
                   $"If you did not request this, please ignore this email.";
            }

            public string GenerateRegistrationConfirmationEmail(string userName, string confirmationLink)
            {
                return $"Hello {userName},<br><br>" +
                   $"Kindly confirm your email by clicking on the link below:<br>" +
                   $"<a href='{confirmationLink}'>Confirm Email</a><br><br>" +
                   $"Thank you!";
            }

            public string GenerateNewPostNotificationEmail(string userName, string postTitle, string postLink)
            {
                   return $@"
                    <h2>Hello {userName},</h2>
                    <p>You have successfully created a new post titled: <strong>{postTitle}</strong>.</p>
                    <p>You can view your post <a href='{postLink}'>here</a>.</p>
                    <p>Thank you for sharing your thoughts with the community!</p>
                    <p>Best regards,<br/>Your Blogging Platform Team</p>
                ";
            }


    }
}
