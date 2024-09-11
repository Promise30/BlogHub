using BloggingAPI.Domain.Entities;

namespace BloggingAPI.Domain.Repositories
{
    public interface IPostTagRepository
    {
        void CreatePostTag(IEnumerable<PostTag> postTags);
    }
}
