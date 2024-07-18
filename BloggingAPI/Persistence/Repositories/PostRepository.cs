using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;
using BloggingAPI.Persistence.Repositories.RepositoryExtensions;
using BloggingAPI.Persistence.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace BloggingAPI.Persistence.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        public void CreatePost(Post post)
        {
            Create(post);
        }

        public void DeletePost(Post post)
        {
            Delete(post);
        }
        public void UpdatePost(Post post)
        {
            Update(post);
        }

        public async Task<PagedList<Post>> GetAllPostsAsync(PostParameters postParameters)
        {

            var startDateAsDateTime = postParameters.StartDate.ToDateTime(TimeOnly.MinValue);
            var endDateAsDateTime = postParameters.EndDate.ToDateTime(TimeOnly.MaxValue);
            var posts = await GetAll()
                .FilterPostsByDatePublished(startDateAsDateTime, endDateAsDateTime)
                .Search(postParameters.SearchTerm)
                .Sort(postParameters.OrderBy)
                .FilterPostsByCategory(postParameters.Category)
                .ToListAsync();
            var count = posts.Count();
            return PagedList<Post>.ToPagedList(posts, postParameters.PageNumber, postParameters.PageSize);
        }
        public async Task<Post> GetPostAsync(int id)
        {
            return await FindByCondition(p => p.Id == id)
                .SingleOrDefaultAsync();
        }
        public async Task<Post> GetPostwithUserAsync(int id)
        {
            return await FindByCondition(p => p.Id == id)
                .Include(p => p.User)
                .SingleOrDefaultAsync();
        }

        public async Task<Post> GetPostWithCommentsAsync(int id)
        {
            return await FindByCondition(p => p.Id.Equals(id))
                                    .Include(c => c.Comment)
                                    .ThenInclude(cv => cv.Votes)
                                    .SingleOrDefaultAsync();
        }
    }
}
