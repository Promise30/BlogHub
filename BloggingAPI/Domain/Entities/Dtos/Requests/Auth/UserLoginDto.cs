using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities.Dtos.Requests.Auth
{
    public record UserLoginDto
    (
        [Required(ErrorMessage = "User name is required")]
        string UserName,
        [Required(ErrorMessage = "Password field is required")]
        string Password 
    );

}
