using BloggingAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public PostCategory Category { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime DateModified { get; set; }
        public string PostImageUrl { get; set; }
        public string ImagePublicId { get; set; }
        public string ImageFormat { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public ICollection<Comment> Comment { get; } = new List<Comment>();
    }
}