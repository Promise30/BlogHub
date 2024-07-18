using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloggingAPI.Domain.Entities
{
    public class CommentVote
    {
        [Key]
        public int Id { get; set; }
        public int CommentId { get; set; }
        public virtual Comment Comment { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public bool? IsUpVote { get; set; }
    }
}
