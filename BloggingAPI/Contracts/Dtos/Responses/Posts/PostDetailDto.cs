using BloggingAPI.Contracts.Dtos.Requests.Tags;

namespace BloggingAPI.Contracts.Dtos.Responses.Posts
{
    public record PostDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public IEnumerable<CommentDto>? Comments { get; set; }
        public IEnumerable<string>? Tags { get; set; }
        public string? PostImageUrl { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime DateModified {  get; set; }
    }
}
