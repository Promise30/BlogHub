using BloggingAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? Author { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime DateModified { get; set; }
        public string? PostImageUrl { get; set; }
        public string? ImagePublicId { get; set; }
        public string? ImageFormat { get; set; }
        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Comment> Comment { get; } = new List<Comment>();
        public virtual ICollection<PostTag> TagLinks { get; set; } = new List<PostTag>();
    }
}