using BloggingAPI.Domain.Enums;
using BloggingAPI.Infrastructure.Validations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities.Dtos.Requests.Auth
{
    public record UserRegistrationDto
    {
        [Required(ErrorMessage ="FirstName is required")]
        public string? FirstName { get; init; }
        [Required(ErrorMessage ="LastName is required")]
        public string? LastName { get; init; }
        [Required(ErrorMessage = "Username is required")]
        public string? UserName { get; init; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; init; }
        [Required(ErrorMessage ="Email is required")]
        [EmailAddress(ErrorMessage = "A valid email address is required")]
        public string? Email { get; init; }
        [Required(ErrorMessage = "A valid phone number is required")]
        [Phone]
        [StringLength(11, ErrorMessage = "Phone number must be 11 digits")]
        public string? PhoneNumber { get; init; }
        [ValidRoles(ErrorMessage ="Invalid role specified")]
        public ICollection<string>? Roles { get; init; }        
    }
}
