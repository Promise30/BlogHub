using BloggingAPI.Domain.Entities;
using BloggingAPI.Persistence.RequestFeatures;

namespace BloggingAPI.Domain.Repositories
{
    public interface IPostRepository
    {
        Task<PagedList<Post>> GetAllPostsAsync(PostParameters postParameters);
        Task<PagedList<Post>> GetAllUserPostsAsync(string ApplicationUserId, PostParameters postParameters);
        Task<Post> GetPostAsync(int id);
        void CreatePost(Post post);
        void DeletePost(Post post);
        void UpdatePost(Post post);
        Task<Post> GetPostWithUserAsync(int id);
        Task<Post> GetPostWithTagsAsync(int id);
        bool PostExists(int id);
    }
}
