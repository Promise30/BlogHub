using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Domain.Repositories
{
    public interface ICommentVoteRepository
    {
        void AddCommentVote(CommentVote commentVote);
        void DeleteCommentVote(CommentVote commentVote);
        void UpdateCommentVote(CommentVote commentVote);
        Task<CommentVote?> GetCommentVoteForCommentAsync(int commentId, string ApplicationUserId);
        Task<(int upvoteCount, int downvoteCount)> GetCommentVoteCountsAsync(int commentId);
    }
}
