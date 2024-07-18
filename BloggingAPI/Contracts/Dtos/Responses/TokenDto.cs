namespace BloggingAPI.Contracts.Dtos.Responses
{
    public record TokenDto
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public DateTime AccessTokenExpiryDate { get; init; }
        public DateTime RefreshTokenExpiryDateExpiry { get; init; }
    }
}
