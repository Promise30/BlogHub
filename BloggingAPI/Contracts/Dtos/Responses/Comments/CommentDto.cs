public class CommentDto 
{
        public int Id { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public int UpVoteCount { get; set; }
        public int DownVoteCount { get; set; } 
        public DateTime PublishedOn { get; set; }
}