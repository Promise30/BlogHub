using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities.Dtos.Requests.Auth
{
    public record ForgotPasswordDto
    (
        [Required(ErrorMessage ="Email is required")]
        [EmailAddress(ErrorMessage ="A valid email address is required")]
         string Email 
    );
}
