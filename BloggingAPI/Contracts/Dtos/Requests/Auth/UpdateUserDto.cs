using BloggingAPI.Contracts.Validations;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Auth
{
    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [RegularExpression(@"^\+\d{1,3}$", ErrorMessage = "Invalid country code format. Use '+' followed by 1-3 digits")]
        public string PhoneCountryCode { get; set; }
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
        public string? PhoneNumber { get; set; }
    }
}
