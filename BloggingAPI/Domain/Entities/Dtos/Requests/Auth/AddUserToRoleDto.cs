using BloggingAPI.Infrastructure.Validations;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities.Dtos.Requests.Auth
{
    public record AddUserToRoleDto
    (
        [Required(ErrorMessage=("Email field is required"))]
        [EmailAddress(ErrorMessage ="A valid email address is required")]
        string Email,
        [ValidRoles(ErrorMessage ="Invalid role specified")]
        ICollection<string>  Roles 
    );
}
