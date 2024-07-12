using BloggingAPI.Constants;
using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Entities.Dtos.Requests.Comments;
using BloggingAPI.Domain.Entities.Dtos.Requests.Posts;
using BloggingAPI.Domain.Entities.Dtos.Responses;
using BloggingAPI.Domain.Entities.Dtos.Responses.Posts;
using BloggingAPI.Domain.Entities.RequestFeatures;
using BloggingAPI.Domain.Enums;
using BloggingAPI.Domain.Repository.Interface;
using BloggingAPI.Extensions;
using BloggingAPI.Infrastructure.Services.Interface;
using BloggingAPI.Network.Interface;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Security.Claims;
using System.Text.Json;

namespace BloggingAPI.Infrastructure.Services.Implementation
{
    public class BloggingService : IBloggingService
    {
        private readonly ILogger<BloggingService> _logger;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly ICloudinaryService _cloudinaryService;
        public BloggingService(ILogger<BloggingService> logger,
                               IRepositoryManager repositoryManager,
                               IHttpContextAccessor httpContextAccessor,
                               IEmailService emailService,
                               IEmailNotificationService emailNotificationService,
                               ICloudinaryService cloudinaryService)
        {
            _logger = logger;
            _repositoryManager = repositoryManager;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _emailNotificationService = emailNotificationService;
            _cloudinaryService = cloudinaryService;
        }
        public async Task<ApiResponse<PostOnlyDto>> CreatePostAsync(CreatePostDto post)
        {
            try 
            { 
                var userId = GetCurrentUserId();
                var author = GetCurrentUserName();

                var file = post?.PostCoverImage;
                ImageUploadResult imageUploadResult = null;
                
                if (file != null)
                {
                    imageUploadResult = await _cloudinaryService.UploadImage(post.PostCoverImage);
                }
                
                var newPost = new Post
                {
                    Title = post.Title,
                    PostImageUrl = imageUploadResult?.Uri?.ToString() ?? string.Empty,
                    ImagePublicId = imageUploadResult.PublicId ?? string.Empty, 
                    ImageFormat = imageUploadResult.Format ?? string.Empty,
                    Category = (PostCategory)Enum.Parse(typeof(PostCategory), post.Category),
                    Author = author,
                    Content = post.Content,
                    UserId = userId,
                };
                _repositoryManager.Post.CreatePost(newPost);
                 await _repositoryManager.SaveAsync();
                _logger.Log(LogLevel.Information, $"Newly created post: Id= {newPost.Id} - Title= {newPost.Title}. Created at: {newPost.PublishedOn.ToShortDateString()}");

                // Schedule a background job to send the email notification
                BackgroundJob.Enqueue(() => _emailNotificationService.SendNewPostNotificationAsync(newPost));

                var postToReturn = newPost.MapToPostOnlyDto();
                return ApiResponse<PostOnlyDto>.Success(201, postToReturn, "Post created successfully");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information,$"Error occured while creating a new post: {ex.Message}");
                return ApiResponse<PostOnlyDto>.Failure(400, "Post could not be created");
            }
        }
        
        public async Task<ApiResponse<object>> DeletePostAsync(int postId)
        {
            try
            {
                var post = await GetPostFromDb(postId);
                if (post is null)
                    return ApiResponse<object>.Failure(404, "Post does not exist.");
                _repositoryManager.Post.DeletePost(post);
                await _repositoryManager.SaveAsync();
                if (!string.IsNullOrEmpty(post.PostImageUrl))
                {
                     var publicId = post.ImagePublicId;
                     var result = await _cloudinaryService.DeleteImageAsync(publicId);
                    _logger.Log(LogLevel.Information, $"Result of the image deletion from cloudinary: {result.JsonObj}");
                }
                _logger.Log(LogLevel.Information, $"Post with Id: {postId} and title {post.Title} has been successfully deleted.");
                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while deleting post: {ex.Message}");
                return ApiResponse<object>.Failure(400, "Request unsuccessful");
            }
        }
        public async Task<ApiResponse<(IEnumerable<PostOnlyDto> posts, MetaData metaData)>> GetAllPostsAsync(PostParameters postParameters)
        {
            try
            {
                var postsWithMetaData = await _repositoryManager.Post.GetAllPostsAsync(postParameters);
                _logger.Log(LogLevel.Information, $"Total posts retrieved from the database: {postsWithMetaData.Count()}");
                var postsDto= postsWithMetaData.Select(p => p.MapToPostOnlyDto()).ToList();
                return ApiResponse<(IEnumerable<PostOnlyDto> posts, MetaData metaData)>.Success(200, (posts: postsDto, metaData: postsWithMetaData.MetaData), "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while retriving post: {ex.Message}");
                return ApiResponse<(IEnumerable<PostOnlyDto> posts, MetaData metaData)>.Failure(500, "Error occured while retrieving posts.");
            }
        }
        public async Task<ApiResponse<PostOnlyDto>> GetPostonlyAsync(int postId)
        {
            try
            {
                var result = await GetPostFromDb(postId);
                if (result is null)
                    return ApiResponse<PostOnlyDto>.Failure(404, "Post does not exist");
                _logger.Log(LogLevel.Information, $"Post with id: {postId} and title '{result.Title}' successfully retrieved from the database as: {JsonSerializer.Serialize(result)}.");
                var postToReturn = result.MapToPostOnlyDto();
                return ApiResponse<PostOnlyDto>.Success(200, postToReturn ,"Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while retrieving post: {ex.Message}");
                return ApiResponse<PostOnlyDto>.Failure(500, "Request unsucessful");
            }
        }
        public async Task<ApiResponse<PostWithCommentsDto>> GetPostWithCommentsAsync(int postId)
        {
            try
            {
                var post = await _repositoryManager.Post.GetPostWithCommentsAsync(postId);
                _logger.Log(LogLevel.Information, $"Post with id: {postId} and title '{post.Title}' successfully retrieved from the database with total comments of {post.Comment.Count()}.");
                var postToReturn = post.MapToPostDto();
                return ApiResponse<PostWithCommentsDto>.Success(200, postToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while retrieving post: {ex.Message}");
                return ApiResponse<PostWithCommentsDto>.Failure(500, "Request unsucessful");
            }
        }
        private async Task<Post> GetPostFromDb(int postId) =>
             await _repositoryManager.Post.GetPostAsync(postId);
        private string GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        }
        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        }

        public async Task<ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>> GetAllCommentsForPostAsync(int postId, CommentParameters commentParameters)
        {
            try
            {
                if (!commentParameters.ValidDateRange)
                {
                    return ApiResponse<(IEnumerable<CommentDto>, MetaData metaData)>.Failure(400, "End date cannot be less than start date");
                }
                var post = await GetPostFromDb(postId);
                if (post is null)
                    return ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>.Failure(404, "Post does not exist.");
                var commentsWithMetadata = await _repositoryManager.Comment.GetCommentsForPostAsync(postId, commentParameters);

                _logger.Log(LogLevel.Information, $"Total comments retrieved from the database for post '{post.Title} is: {commentsWithMetadata.Count()}");
                var commentsDto = commentsWithMetadata.Select(c=> new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Author = c.Author,
                    PublishedOn = c.PublishedOn,
                }).ToList();
                return ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>.Success(200, (comments: commentsDto, metaData: commentsWithMetadata.MetaData) , "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while retrieving comments: {ex.Message}");
               return ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>.Failure(500, "Request unsucessful");
            }
        }

        public async Task<ApiResponse<CommentDto>> GetCommentForPostAsync(int postId, int commentId)
        {
            try
            {
                var post = await GetPostFromDb(postId);
                if (post == null)
                    return ApiResponse<CommentDto>.Failure(404, "Post does not exist");
                var comment = await _repositoryManager.Comment.GetCommentForPostAsync(postId, commentId);
                if (comment is null)
                    return ApiResponse<CommentDto>.Failure(404, "Comment does not exist");
                var commentToReturn = new CommentDto
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    Author = comment.Author,
                    PublishedOn = comment.PublishedOn
                };
                _logger.Log(LogLevel.Information, $"Comment with Id: {commentToReturn.Id} for Post with Id: {post.Id} successfully retrieved from the database");
                return ApiResponse<CommentDto>.Success(200, commentToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while retrieving comment: {ex.Message}");
                return ApiResponse<CommentDto>.Failure(500, "Request unsucessful");
            }
        }

        public async Task<ApiResponse<CommentDto>> CreateCommentForPostAsync(int postId, CreateCommentDto createCommentDto)
        {
            try
            {
                
                var post = await GetPostFromDb(postId);
                if (post is null)
                    return ApiResponse<CommentDto>.Failure(404, "Post does not exist");
                var commentToCreate = new Comment
                {
                    Content = createCommentDto.Content,
                    Author = GetCurrentUserName() ?? "Anonymous",
                };
                _repositoryManager.Comment.CreateCommentForPost(postId, commentToCreate);
                await _repositoryManager.SaveAsync();
                var commentToReturn = new CommentDto
                {
                    Id = commentToCreate.Id,
                    Content = commentToCreate.Content,
                    Author = commentToCreate.Author,
                    PublishedOn = commentToCreate.PublishedOn
                };
                _logger.Log(LogLevel.Information, $"Newly created comment for post with id: {post.Id} is: {JsonSerializer.Serialize(commentToReturn)}");
                // Schedule a background job to send the email notification
                BackgroundJob.Enqueue(() => _emailNotificationService.SendNewCommentNotificationAsync(postId, commentToCreate));

                return ApiResponse<CommentDto>.Success(201, commentToReturn, "Comment created successfully");

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while creating comment: {ex.Message}");
                return ApiResponse<CommentDto>.Failure(500, "Request unsucessful");
            }
        }

        public async Task<ApiResponse<object>> DeleteCommentForPostAsync(int postId, int commentId)
        {
            try
            {
                var post = await GetPostFromDb(postId);
                if (post is null)
                    return ApiResponse<object>.Failure(404, "Post does not exist");
                var commmentToDelete = await _repositoryManager.Comment.GetCommentForPostAsync(postId, commentId);
                if (commmentToDelete is null)
                    return ApiResponse<object>.Failure(404, "Comment does not exist");
                _repositoryManager.Comment.DeleteComment(commmentToDelete);
                await _repositoryManager.SaveAsync();
                _logger.Log(LogLevel.Information, $"Comment with Id: {commmentToDelete.Id} successfully deleted from the database");
                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {

                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while deleting comment: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsucessful");
            }
        }

        public async Task<ApiResponse<object>> UpdateCommentForPostAsync(int postId, int commentId, UpdateCommentDto updateCommentDto)
        {
            try
            {
                var post = await GetPostFromDb(postId);
                if (post is null)
                    return ApiResponse<object>.Failure(404, "Post does not exist");
                var commentEntity = await _repositoryManager.Comment.GetCommentForPostAsync(postId, commentId);
                if (commentEntity is null)
                    return ApiResponse<object>.Failure(404, "Comment does not exist");
                commentEntity.Content = updateCommentDto.Content;

                _repositoryManager.Comment.UpdateCommentForPost(commentEntity);
                await _repositoryManager.SaveAsync();
                _logger.Log(LogLevel.Information, $"Newly updated comment for Post with Id: {post.Id} is: {JsonSerializer.Serialize(commentEntity)}");
                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while updating comment: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsucessful");
            }
        }

        public async Task<ApiResponse<object>> UpdatePostAsync(int postId, UpdatePostDto updatePostDto)
        {
            try
            {
                var postEntity = await GetPostFromDb(postId);
                if (postEntity is null)
                    return ApiResponse<object>.Failure(404, "Post does not exist");

                if(updatePostDto.PostCoverImage != null)
                {
                    if (!string.IsNullOrEmpty(postEntity.PostImageUrl))
                    {
                        var publicId = postEntity.ImagePublicId;
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                    // upload the new image to cloudinary
                    var uploadResult = await _cloudinaryService.UploadImage(updatePostDto.PostCoverImage);
                    _logger.Log(LogLevel.Information, $"Result of the new image upload: {uploadResult.JsonObj}");
                    postEntity.PostImageUrl = uploadResult.Uri.AbsoluteUri;
                    postEntity.ImagePublicId = uploadResult.PublicId;
                    postEntity.ImageFormat = uploadResult.Format;
                }
                //postEntity.Id = postId;
                postEntity.Title = updatePostDto.Title;
                postEntity.Content = updatePostDto.Content;
                postEntity.Category = (PostCategory)Enum.Parse(typeof(PostCategory),updatePostDto.Category);
            
                _logger.Log(LogLevel.Information, $"Newly updated post: {JsonSerializer.Serialize(postEntity)}");

                _repositoryManager.Post.UpdatePost(postEntity);

                await _repositoryManager.SaveAsync();
                _logger.Log(LogLevel.Information, $"Newly updated post with Id: {postEntity.Id} is: {JsonSerializer.Serialize(postEntity)}");
                return ApiResponse<object>.Success(204, null);


            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occured while updating post: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsucessful");
            }
        }
    }
}
