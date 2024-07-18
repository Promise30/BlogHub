namespace BloggingAPI.Services.Constants
{
    public static class EmailResponseMessage
    {
        public static string GetEmailSuccessMessage(string emailAddress) => $"Email sent successfully to {emailAddress}";

    }
}
