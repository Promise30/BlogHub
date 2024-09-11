using System.Globalization;

namespace BloggingAPI.Contracts.Dtos.Responses.Auth
{
    public class UserResponseDto
    {
        public string Id { get; set; }
        public string UserName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneCountryCode { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
