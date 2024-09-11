namespace BloggingAPI.Domain.Repositories
{
    public interface IRepositoryManager
    {
        IPostRepository Post { get; }
        ICommentRepository Comment { get; }
        ICommentVoteRepository CommentVote { get; }
        ITagRepository Tag { get; }
        IPostTagRepository PostTag { get; }
        Task SaveAsync();
    }
}
