namespace BloggingAPI.Contracts.Dtos.Responses.Posts
{
    public record PostOnlyDto
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Content { get; init; }
        public string Author { get; init; }
        public string Category { get; init; }
        public string PostImageUrl { get; init; }
        public DateTime PublishedOn { get; init; }
    }
}
