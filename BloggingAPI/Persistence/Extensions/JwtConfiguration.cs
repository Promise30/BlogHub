namespace BloggingAPI.Persistence.Extensions
{
    public class JwtConfiguration
    {
        public string validIssuer { get; set; } = null!;
        public string validAudience { get; set; } = null!;
        public string secretKey { get; set; } = null!;
        public string expires { get; set; } = null!;

    }
}
