 using BloggingAPI.Contracts.Validations;
using BloggingAPI.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; } 
        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; } 
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } 
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "A valid email address is required")]
        public string Email { get; set; } 
        [Required(ErrorMessage ="Phone country code is required")]
        [RegularExpression(@"^\+\d{1,3}$", ErrorMessage = "Invalid country code format. Use '+' followed by 1-3 digits")]
        public string PhoneCountryCode { get; set; }
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
        public string? PhoneNumber { get; set; }
        [ValidRoles(ErrorMessage = "Invalid role specified")]
        public ICollection<string>? Roles { get; set; }
    }
}
