using BloggingAPI.Contracts.Dtos.Requests.Comments;
using BloggingAPI.Contracts.Dtos.Requests.Posts;
using BloggingAPI.Contracts.Dtos.Requests.Tags;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Posts;
using BloggingAPI.Contracts.Dtos.Responses.Tags;
using BloggingAPI.Domain.Entities;
using BloggingAPI.Persistence.RequestFeatures;

namespace BloggingAPI.Services.Interface
{
    public interface IBloggingService
    {
        // Posts
        Task<ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>> GetAllPostsAsync(PostParameters postParameters);
        Task<ApiResponse<PostDetailDto>> GetPostAsync(int postId);
        Task<ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>> GetAllUserPostsAsync(PostParameters postParameters);
        Task<ApiResponse<NewPostDto>> CreatePostAsync(CreatePostDto post);
        Task<ApiResponse<object>> UpdatePostAsync(int postId, UpdatePostDto updatePostDto);
        Task<ApiResponse<object>> UpdatePostCoverImageAsync(int postId, UpdatePostCoverImageDto updatePostCoverImageDto);
        Task<ApiResponse<object>> DeletePostAsync(int postId);
        
        // Comments
        Task<ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>> GetAllCommentsForPostAsync(int postId, CommentParameters commentParameters);
        Task<ApiResponse<CommentDto>> GetCommentForPostAsync(int postId, int commentId);
        Task<ApiResponse<CommentDto>> CreateCommentForPostAsync(int postId, CreateCommentDto createCommentDto);
        Task<ApiResponse<object>> UpdateCommentForPostAsync(int postId, int commentId, UpdateCommentDto updateCommentDto);
        Task<ApiResponse<object>> DeleteCommentForPostAsync(int postId, int commentId);
        Task<ApiResponse<object>> VoteCommentAsync(int postId, int commentId, bool? isUpVote);

        // Tags
        Task<ApiResponse<TagDto>> CreateTagAsync(CreateTagDto createTag);
        Task<ApiResponse<IEnumerable<TagDto>>> GetAllTagsAsync();
        Task<ApiResponse<TagDto>> GetTagByIdAsync(int tagId);
        Task<ApiResponse<IEnumerable<PostDetailDto>>> GetAllPostsForTagAsync(int tagId);
        Task<ApiResponse<object>> UpdateTagAsync(int tagId, UpdateTagDto tagDto);
        Task<ApiResponse<object>> DeleteTagAsync(int tagId);
    }
}
