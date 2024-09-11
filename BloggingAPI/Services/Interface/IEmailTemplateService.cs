namespace BloggingAPI.Services.Interface
{
    public interface IEmailTemplateService
    {
       
            public string GenerateRegistrationConfirmationEmail(string userName, string confirmationLink);
            public string GeneratePasswordResetEmail(string userName, string passwordResetLink);
            public string GenerateEmailChangeConfirmationLink(string userName, string confirmationLink);
            public string GenerateNewPostNotificationEmail(string userName, string postTitle,  string postLink);
    }
}
