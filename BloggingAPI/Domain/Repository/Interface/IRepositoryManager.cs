namespace BloggingAPI.Domain.Repository.Interface
{
    public interface IRepositoryManager
    {
        IPostRepository Post { get; }
        ICommentRepository Comment { get; }
        Task SaveAsync();
    }
}
