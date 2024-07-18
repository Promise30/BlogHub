using BloggingAPI.Domain.Entities;
using BloggingAPI.Persistence.RequestFeatures;

namespace BloggingAPI.Domain.Repositories
{
    public interface IPostRepository
    {
        Task<PagedList<Post>> GetAllPostsAsync(PostParameters postParameters);
        Task<Post> GetPostAsync(int id);
        Task<Post> GetPostWithCommentsAsync(int id);
        void CreatePost(Post post);
        void DeletePost(Post post);
        void UpdatePost(Post post);
        Task<Post> GetPostwithUserAsync(int id);
    }
}
