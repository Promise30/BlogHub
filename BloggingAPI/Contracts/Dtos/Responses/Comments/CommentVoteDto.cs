namespace BloggingAPI.Contracts.Dtos.Responses.Comments
{
    public class CommentVoteDto
    {
        public int CommentId { get; set; }
        public bool? IsUpVote { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
    }
}
