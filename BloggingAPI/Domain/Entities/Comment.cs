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
        public virtual ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();

        // Computed properties
        public int UpVoteCount => Votes?.Count(v => v.IsUpVote == true) ?? 0;
        public int DownVoteCount => Votes?.Count(v => v.IsUpVote == false) ?? 0;

    }
}