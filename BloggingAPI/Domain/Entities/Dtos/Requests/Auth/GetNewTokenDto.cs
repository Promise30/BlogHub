using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities.Dtos.Requests.Auth
{
    public record GetNewTokenDto
    {
        [Required(ErrorMessage = "Access token is required")]
        public string AccessToken { get; init; }
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; init; }
    }

}
