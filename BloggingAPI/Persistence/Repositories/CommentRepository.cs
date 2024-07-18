using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;
using BloggingAPI.Persistence.Repositories.RepositoryExtensions;
using BloggingAPI.Persistence.RequestFeatures;
using Microsoft.EntityFrameworkCore;

namespace BloggingAPI.Persistence.Repositories
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
        public void CreateCommentForPost(int postId, Comment comment)
        {
            comment.PostId = postId;
            Create(comment);
        }
        public void UpdateCommentForPost(Comment comment)
        {
            Update(comment);
        }

        public void DeleteComment(Comment comment)
        {
            Delete(comment);
        }
        public async Task<Comment> GetCommentForPostAsync(int postId, int commentId)
        {
            return await FindByCondition(c => c.PostId == postId && c.Id == commentId)
                    .SingleOrDefaultAsync();
        }

        public async Task<PagedList<Comment>> GetCommentsForPostAsync(int postId, CommentParameters commentParameters)
        {
            var startDateAsDateTime = commentParameters.StartDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;
            var endDateAsDateTime = commentParameters.EndDate?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.MaxValue;
            var comments = await FindByCondition(c => c.PostId == postId)
                    .FilterComments(startDateAsDateTime, endDateAsDateTime)
                    .Search(commentParameters.SearchTerm)
                    .ToListAsync();
            var count = comments.Count();
            return PagedList<Comment>.ToPagedList(comments, commentParameters.PageNumber, commentParameters.PageSize);

        }
    }
}
