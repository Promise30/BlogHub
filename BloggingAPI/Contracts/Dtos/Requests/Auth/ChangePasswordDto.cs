using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage ="The current password field is required")]
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage ="The new password field is required")]
        public string NewPassword { get; set; }
    }
}
