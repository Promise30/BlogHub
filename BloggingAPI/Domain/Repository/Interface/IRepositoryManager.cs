namespace BloggingAPI.Domain.Repository.Interface
{
    public interface IRepositoryManager
    {
        IPostRepository Post { get; }
        ICommentRepository Comment { get; }
        ICommentVoteRepository CommentVote { get; }
        Task SaveAsync();
    }
}
