using System.Globalization;

namespace BloggingAPI.Contracts.Dtos.Responses
{
    public class UserResponseDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
