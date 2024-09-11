using BloggingAPI.Contracts.Dtos.Requests.Tags;

namespace BloggingAPI.Contracts.Dtos.Responses.Posts
{
    public class NewPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public string? PostImageUrl { get; set; }
        public IEnumerable<string>? Tags { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime DateModified { get; set; }
    }
}