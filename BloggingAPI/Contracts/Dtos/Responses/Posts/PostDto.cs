using BloggingAPI.Contracts.Dtos.Requests.Tags;

namespace BloggingAPI.Contracts.Dtos.Responses.Posts
{
    public class PostDto
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string? Content { get; init; }
        public string Author { get; init; }
        public IEnumerable<string>? Tags { get; init; }
        public string? PostImageUrl { get; init; }
        public DateTime PublishedOn { get; init; }
    }
}
