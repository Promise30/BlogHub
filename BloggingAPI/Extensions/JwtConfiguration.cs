namespace BloggingAPI.Extensions
{
    public class JwtConfiguration
    {
        public string validIssuer { get; set; }
        public string validAudience { get; set; }
        public string secretKey { get; set; }
        public string expires { get; set; }

    }
}
