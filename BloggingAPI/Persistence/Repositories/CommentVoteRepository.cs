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

        public async Task<CommentVote> GetCommentVoteForCommentAsync(int commentId, string userId) =>
            await FindByCondition(c => c.CommentId == commentId && c.UserId == userId).SingleOrDefaultAsync();

        public void UpdateCommentVote(CommentVote commentVote)
        {
            Update(commentVote);
        }
    }
}
