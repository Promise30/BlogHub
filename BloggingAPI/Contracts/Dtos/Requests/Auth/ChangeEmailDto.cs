using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public class ChangeEmailDto
    {
        [Required(ErrorMessage = "The current email address field is required")]
        public string CurrentEmail { get; set; }
        [Required(ErrorMessage = "The new email field is required")]
        public string NewEmail { get; set; }
    }
}
