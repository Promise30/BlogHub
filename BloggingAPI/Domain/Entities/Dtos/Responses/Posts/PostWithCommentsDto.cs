namespace BloggingAPI.Domain.Entities.Dtos.Responses.Posts
{
    public record PostWithCommentsDto
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Content { get; init; }
        public string Author { get; init; }
        public string Category { get; init; }
        public string PostImageUrl { get; init; }
        public string PublishedOn { get; init; }
        public IEnumerable<CommentDto> Comments { get; init; } = new List<CommentDto>();
    };
}
