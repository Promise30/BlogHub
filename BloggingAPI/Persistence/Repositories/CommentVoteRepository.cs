using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BloggingAPI.Persistence.Repositories
{
    public class CommentVoteRepository : BaseRepository<CommentVote>, ICommentVoteRepository
    {
        public CommentVoteRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        public void AddCommentVote(CommentVote commentVote)
        {
            Create(commentVote);

        }

        public void DeleteCommentVote(CommentVote commentVote)
        {
            Delete(commentVote);
        }

        public async Task<CommentVote?> GetCommentVoteForCommentAsync(int commentId, string ApplicationUserId) =>
            await FindByCondition(c => c.CommentId == commentId && c.ApplicationUserId == ApplicationUserId).SingleOrDefaultAsync();
        public void UpdateCommentVote(CommentVote commentVote)
        {
            Update(commentVote);
        }
        public async Task<(int upvoteCount, int downvoteCount)> GetCommentVoteCountsAsync(int commentId)
        {
            var voteCounts = await FindByCondition(cv => cv.CommentId == commentId)
                .GroupBy(cv => cv.IsUpVote)
                .Select(g => new { IsUpVote = g.Key, Count = g.Count() })
                .ToListAsync();

            var upvoteCount = voteCounts.FirstOrDefault(vc => vc.IsUpVote == true)?.Count ?? 0;
            var downvoteCount = voteCounts.FirstOrDefault(vc => vc.IsUpVote == false)?.Count ?? 0;

            return (upvoteCount, downvoteCount);
        }
    }
}
