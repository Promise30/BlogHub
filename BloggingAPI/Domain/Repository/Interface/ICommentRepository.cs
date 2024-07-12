using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Entities.RequestFeatures;
using System.Threading.Tasks;

namespace BloggingAPI.Domain.Repository.Interface
{
    public interface ICommentRepository
    {
        Task<PagedList<Comment>> GetCommentsForPostAsync(int postId, CommentParameters commentParameters);
        Task<Comment> GetCommentForPostAsync(int postId, int commentId);
        void CreateCommentForPost(int postId, Comment comment);
        void DeleteComment(Comment comment);
        void UpdateCommentForPost(Comment comment);
    }
}
