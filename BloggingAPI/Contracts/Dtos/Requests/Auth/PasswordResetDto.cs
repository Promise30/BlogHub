using BloggingAPI.Contracts.Validations;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public class PasswordResetDto
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "A valid email address is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password field is required")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Password confirmation field is required")]
        [PasswordConfirmationValidation("NewPassword", ErrorMessage = "Password fields do not match")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Token field is required")]
        public string Token { get; set; }
    }
}
