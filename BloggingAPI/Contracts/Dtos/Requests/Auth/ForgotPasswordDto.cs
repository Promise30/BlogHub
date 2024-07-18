using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public record ForgotPasswordDto
    (
        [Required(ErrorMessage ="Email is required")]
        [EmailAddress(ErrorMessage ="A valid email address is required")]
         string Email
    );
}
