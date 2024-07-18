using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Domain.Repository.Interface
{
    public interface ICommentVoteRepository
    {
        void AddCommentVote(CommentVote commentVote);
        void DeleteCommentVote(CommentVote commentVote);
        void UpdateCommentVote(CommentVote commentVote);
        Task<CommentVote> GetCommentVoteForCommentAsync(int commentId, string userId);
    }
}
