using BloggingAPI.Contracts.Validations;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public class AddUserToRoleDto
    {
        [Required(ErrorMessage = "Email field is required")]
        [EmailAddress(ErrorMessage = "A valid email address is required")]
        public string Email { get; set; }
        [ValidRoles(ErrorMessage = "Invalid role specified")]
        public ICollection<string> Roles { get; set; }

    }
}
