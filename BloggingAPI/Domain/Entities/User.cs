using Microsoft.AspNetCore.Identity;

namespace BloggingAPI.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<Post> Posts { get; } = new List<Post>();
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryDate { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
