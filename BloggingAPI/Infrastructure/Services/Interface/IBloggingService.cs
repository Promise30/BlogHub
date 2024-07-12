using BloggingAPI.Domain.Entities.Dtos.Requests.Comments;
using BloggingAPI.Domain.Entities.Dtos.Requests.Posts;
using BloggingAPI.Domain.Entities.Dtos.Responses;
using BloggingAPI.Domain.Entities.Dtos.Responses.Posts;
using BloggingAPI.Domain.Entities.RequestFeatures;

namespace BloggingAPI.Infrastructure.Services.Interface
{
    public interface IBloggingService
    {
        // Posts
        Task<ApiResponse<(IEnumerable<PostOnlyDto> posts, MetaData metaData)>> GetAllPostsAsync(PostParameters postParameters);
        Task<ApiResponse<PostOnlyDto>> GetPostonlyAsync(int postId);
        Task<ApiResponse<PostWithCommentsDto>> GetPostWithCommentsAsync(int postId);
        Task<ApiResponse<PostOnlyDto>> CreatePostAsync(CreatePostDto post);
        Task<ApiResponse<object>> UpdatePostAsync(int postId, UpdatePostDto updatePostDto); 
        Task<ApiResponse<object>> DeletePostAsync(int postId);

        // Comments
        Task<ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>> GetAllCommentsForPostAsync(int postId, CommentParameters commentParameters);
        Task<ApiResponse<CommentDto>> GetCommentForPostAsync(int postId, int commentId);
        Task<ApiResponse<CommentDto>> CreateCommentForPostAsync(int postId, CreateCommentDto createCommentDto);
        Task<ApiResponse<object>> UpdateCommentForPostAsync(int postId, int commentId, UpdateCommentDto updateCommentDto);
        Task<ApiResponse<object>> DeleteCommentForPostAsync(int postId, int commentId);
    }
}
