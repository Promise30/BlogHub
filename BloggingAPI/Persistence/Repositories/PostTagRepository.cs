using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;

namespace BloggingAPI.Persistence.Repositories
{
    public class PostTagRepository : BaseRepository<PostTag>, IPostTagRepository
    {
        public PostTagRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        public void CreatePostTag(IEnumerable<PostTag> postTags)
        {
            CreateRange(postTags);
        }
    }
}

