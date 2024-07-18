using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public record UserLoginDto
    (
        [Required(ErrorMessage = "User name is required")]
        string UserName,
        [Required(ErrorMessage = "Password field is required")]
        string Password
    );

}
