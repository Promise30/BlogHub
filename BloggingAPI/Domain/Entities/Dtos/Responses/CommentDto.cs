public record CommentDto {
    public int Id { get; init; }
     public string Content { get; init; }
     public string Author { get; init; }
     public DateTime PublishedOn { get; init; }
}