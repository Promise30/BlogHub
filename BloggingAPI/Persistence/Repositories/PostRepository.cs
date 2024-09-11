using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;
using BloggingAPI.Persistence.Repositories.RepositoryExtensions;
using BloggingAPI.Persistence.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BloggingAPI.Persistence.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
        public void CreatePost(Post post) => Create(post);
        public void DeletePost(Post post) => Delete(post);
        public void UpdatePost(Post post) => Update(post);
        public async Task<PagedList<Post>> GetAllPostsAsync(PostParameters postParameters)
        {
          
            var startDateAsDateTime = postParameters.StartDate.ToDateTime(TimeOnly.MinValue);
            var endDateAsDateTime = postParameters.EndDate.ToDateTime(TimeOnly.MaxValue);

            var query = GetAll()
                    .FilterPostsByDatePublished(startDateAsDateTime, endDateAsDateTime)
                    .Search(postParameters.SearchTerm)
                    .FilterPostsByTag(postParameters.Tag)
                    .Include(p => p.TagLinks)
                        .ThenInclude(pt => pt.Tag);
            var count = await query.CountAsync();   
            var posts = await query
                    .Sort(postParameters.OrderBy)
                    .Skip((postParameters.PageNumber - 1) * postParameters.PageSize)
                    .Take(postParameters.PageSize)
                    .ToListAsync();
            return new PagedList<Post>(posts, count, postParameters.PageNumber, postParameters.PageSize);
        }
        public async Task<PagedList<Post>> GetAllUserPostsAsync(string ApplicationUserId, PostParameters postParameters)
        {

            var startDateAsDateTime = postParameters.StartDate.ToDateTime(TimeOnly.MinValue);
            var endDateAsDateTime = postParameters.EndDate.ToDateTime(TimeOnly.MaxValue);
            var query = FindByCondition(p=> p.ApplicationUserId == ApplicationUserId)
                    .FilterPostsByDatePublished(startDateAsDateTime, endDateAsDateTime)
                    .Search(postParameters.SearchTerm)
                    .FilterPostsByTag(postParameters.Tag)
                    .Include(p => p.TagLinks)
                        .ThenInclude(pt => pt.Tag);
            var count = await query.CountAsync();
            var posts = await query
                    .Sort(postParameters.OrderBy)
                    .Skip((postParameters.PageNumber - 1) * postParameters.PageSize)
                    .Take(postParameters.PageSize)
                    .ToListAsync();
            return new PagedList<Post>(posts, count, postParameters.PageNumber, postParameters.PageSize);
        }
        public async Task<Post> GetPostAsync(int id) => await FindByCondition(p => p.PostId == id)
                                                                .Include(p=> p.TagLinks)
                                                                        .ThenInclude(pt => pt.Tag)
                                                                .Include(c=> c.Comment)
                                                                        .ThenInclude(cv=> cv.Votes)
                                                                .SingleAsync();
        public async Task<Post> GetPostWithUserAsync(int id) => await FindByCondition(p => p.PostId == id)
                                                                        .Include(p => p.User)
                                                                        .SingleAsync();
        public async Task<Post> GetPostWithTagsAsync(int id) => await FindByCondition(p => p.PostId == id)
                                                                        .Include(p => p.TagLinks)
                                                                        .SingleAsync();
        public bool PostExists(int id) => FindByCondition(p => p.PostId == id).Any();
    }
}
