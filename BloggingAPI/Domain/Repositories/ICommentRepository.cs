using BloggingAPI.Domain.Entities;
using BloggingAPI.Persistence.RequestFeatures;
using System.Threading.Tasks;

namespace BloggingAPI.Domain.Repositories
{
    public interface ICommentRepository
    {
        Task<PagedList<Comment>> GetCommentsForPostAsync(int postId, CommentParameters commentParameters);
        Task<Comment?> GetCommentForPostAsync(int commentId);
        Task<Comment?> GetComment(int commentId);   
        void CreateCommentForPost(int postId, Comment comment);
        void DeleteComment(Comment comment);
        void UpdateCommentForPost(Comment comment);

    }
}
