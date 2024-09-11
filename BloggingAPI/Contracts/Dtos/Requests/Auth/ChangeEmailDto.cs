using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public class ChangeEmailDto
    {
        
        [Required(ErrorMessage = "The new email field is required")]
        public string NewEmail { get; set; }
    }
}
