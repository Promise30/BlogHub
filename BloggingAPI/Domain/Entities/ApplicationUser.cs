using Microsoft.AspNetCore.Identity;

namespace BloggingAPI.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneCountryCode { get; set; } = null!;
        public virtual ICollection<Post> Posts { get; } = new List<Post>();
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryDate { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
