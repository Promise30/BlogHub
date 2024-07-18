using BloggingAPI.Contracts.Dtos.Requests.Comments;
using BloggingAPI.Contracts.Dtos.Requests.Posts;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Posts;
using BloggingAPI.Domain.Entities;
using BloggingAPI.Persistence.RequestFeatures;

namespace BloggingAPI.Services.Interface
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

        Task<ApiResponse<object>> VoteCommentAsync(int postId, int commentId, bool? isUpVote);
    }
}
