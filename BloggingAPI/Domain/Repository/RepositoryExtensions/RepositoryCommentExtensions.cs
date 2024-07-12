using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Domain.Repository.RepositoryExtensions
{
    public static class RepositoryCommentExtensions
    {
        public static IQueryable<Comment> FilterComments(this IQueryable<Comment> comments, DateTime startDate, DateTime endDate) => comments.Where(c => (c.PublishedOn >= startDate && c.PublishedOn <= endDate));
        public static IQueryable<Comment> Search(this IQueryable<Comment> comments, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return comments;

            return comments.Where(e => e.Content.Contains(searchTerm));
        }
    }
}
