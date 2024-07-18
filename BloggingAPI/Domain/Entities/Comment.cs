using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime DateModified { get; set; }
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}