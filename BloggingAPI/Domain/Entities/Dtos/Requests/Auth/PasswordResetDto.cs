using BloggingAPI.Infrastructure.Validations;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities.Dtos.Requests.Auth
{
    public record PasswordResetDto
    (
        [Required(ErrorMessage ="Email address is required")]
        [EmailAddress(ErrorMessage ="A valid email address is required")]
         string Email,
        [Required(ErrorMessage ="Password field is required")]
         string Password,
        [Required(ErrorMessage ="Password confirmation field is required")]
        [PasswordConfirmationValidation("Password", ErrorMessage = "Password fields do not match")]
         string ConfirmPassword,
        [Required(ErrorMessage ="Token field is required")]
         string Token
    );
}
