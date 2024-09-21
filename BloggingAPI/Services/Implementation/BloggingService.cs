using BloggingAPI.Contracts.Dtos.Requests.Comments;
using BloggingAPI.Contracts.Dtos.Requests.Posts;
using BloggingAPI.Contracts.Dtos.Requests.Tags;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Comments;
using BloggingAPI.Contracts.Dtos.Responses.Posts;
using BloggingAPI.Contracts.Dtos.Responses.Tags;
using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;
using BloggingAPI.Persistence.Extensions;
using BloggingAPI.Persistence.RequestFeatures;
using BloggingAPI.Services.Interface;
using CloudinaryDotNet.Actions;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace BloggingAPI.Services.Implementation
{
    public class BloggingService : IBloggingService
    {
        private readonly ILogger<BloggingService> _logger;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUrlHelper _urlHelper;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly UserManager<ApplicationUser> _userManager;
       
        public BloggingService(ILogger<BloggingService> logger,
                               IRepositoryManager repositoryManager,
                               IHttpContextAccessor httpContextAccessor,
                               IEmailService emailService,
                               ICloudinaryService cloudinaryService,
                               IUrlHelper urlHelper,
                               IEmailTemplateService emailTemplateService,
                               IRedisCacheService redisCacheService,
                               UserManager<ApplicationUser> userManager
            )
        {
            _logger = logger;
            _repositoryManager = repositoryManager;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _cloudinaryService = cloudinaryService;
            _urlHelper = urlHelper;
            _emailTemplateService = emailTemplateService;
            _redisCacheService = redisCacheService;
            _userManager = userManager;
        }
        public async Task<ApiResponse<NewPostDto>> CreatePostAsync(CreatePostDto post)
        {
            try
            {
                var ApplicationUserId = GetCurrentApplicationUserId();
                var author = GetCurrentUserName();
                var currentUserEmail = GetCurrentUserEmail();
                var file = post?.PostCoverImage;
                ImageUploadResult imageUploadResult = null;
                if (file != null)
                {
                    imageUploadResult = await _cloudinaryService.UploadImage(file);
                }
                var uniqueTags = post?.TagsId?.Distinct().ToList();
                var tags =  _repositoryManager.Tag.GetTagsByIds(uniqueTags!);

                var newPost = new Post
                {
                    Title = post.Title,
                    PostImageUrl = imageUploadResult?.Url?.ToString(),
                    ImagePublicId = imageUploadResult?.PublicId ,
                    ImageFormat = imageUploadResult?.Format,
                    Author = author,
                    Content = post.Content,
                    ApplicationUserId = ApplicationUserId,
                };
               
                var postTags = tags.Select(tag => new PostTag { Post = newPost, TagId = tag.TagId }).ToList();
  
                _repositoryManager.PostTag.CreatePostTag(postTags);
                await _repositoryManager.SaveAsync();

                _logger.Log(LogLevel.Information, "Newly created post: {title}. Created at: {time}", newPost.Title, newPost.PublishedOn.ToShortDateString());

                // Generate email content and setup a background task to handle it
                var postLink = _urlHelper.Action("GetPost", "Blog", new { id = newPost.PostId }, _httpContextAccessor?.HttpContext?.Request.Scheme);
                var emailContent = _emailTemplateService.GenerateNewPostNotificationEmail(newPost.Author, newPost.Title, postLink);
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(currentUserEmail, "New Post Notification!", emailContent));

                var postToReturn = newPost.ToNewlyCreatedPost(tags);
                return ApiResponse<NewPostDto>.Success(201, postToReturn, "Post created successfully");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while creating a new post: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<NewPostDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<object>> DeletePostAsync(int postId)
        {
            try
            {
                var post = await GetPostFromDb(postId);
                if (post is null)
                    return ApiResponse<object>.Failure(404, "Post does not exist.");

                // check eligibility to delete a post
                var currentApplicationUserId = GetCurrentApplicationUserId();
                var currentUserRoles = GetCurrentUserRoles();

                if (currentUserRoles is null || currentUserRoles is null)
                    return ApiResponse<object>.Failure(401, "User authentication failed.");
                if (currentApplicationUserId != post.ApplicationUserId && !currentUserRoles.Contains("Administrator"))
                    return ApiResponse<object>.Failure(403, "You do not have the permission to perform this action");

                _repositoryManager.Post.DeletePost(post);
                await _repositoryManager.SaveAsync();

                if (!string.IsNullOrEmpty(post.PostImageUrl))
                {
                    var publicId = post.ImagePublicId;
                    var jobId = BackgroundJob.Enqueue(() =>  _cloudinaryService.DeleteImageAsync(publicId));
                    
                    _logger.Log(LogLevel.Information, $"Result of the image deletion job {jobId} for image {publicId}.");
                }
                _logger.Log(LogLevel.Information, "Post with {id} has been successfully deleted.", postId);
                return ApiResponse<object>.Success(204, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while deleting post: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>> GetAllPostsAsync(PostParameters postParameters)
        {
            try
            {
                    var postsWithMetaData = await _repositoryManager.Post.GetAllPostsAsync(postParameters);
                    _logger.LogInformation("Total posts retrieved from the database: {count}", postsWithMetaData.Count());

                    var postsDto = postsWithMetaData.Select(p => p.ToPostDto()).ToList();
                     return ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>.Success(200, (postsDto, postsWithMetaData.MetaData), "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving posts from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<(IEnumerable<PostDto>, MetaData metaData)>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>> GetAllUserPostsAsync(PostParameters postParameters)
        {
            try
            {
                var ApplicationUserId = GetCurrentApplicationUserId();
                var postsWithMetaData = await _repositoryManager.Post.GetAllUserPostsAsync(ApplicationUserId, postParameters);
                _logger.Log(LogLevel.Information, "Total posts retrieved from the database: {count}", postsWithMetaData.Count());
              
                var postsDto = postsWithMetaData.Select(p => p.ToPostDto()).ToList();
                return ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>.Success(200, (posts: postsDto, metaData: postsWithMetaData.MetaData), "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving user posts from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<PostDetailDto>> GetPostAsync(int postId)
        {
            try
            {
                var cacheKey = $"{GetPostCacheKey()}_{postId}";
                var post = await _redisCacheService.GetCachedDataAsync<Post>(cacheKey);
                if (post is null)
                {
                    post = await GetPostFromDb(postId);
                    if (post is null)
                        return ApiResponse<PostDetailDto>.Failure(404, "Post not found");
                    _logger.Log(LogLevel.Information, "Post successfully retrieved from the database");
                    await _redisCacheService.SetCachedDataAsync(cacheKey, post, TimeSpan.FromMinutes(3));
                }
                else
                {
                    _logger.Log(LogLevel.Information, "Post details retrieved from cache");
                }
                var postToReturn = post.ToPostDetailDto();
                return ApiResponse<PostDetailDto>.Success(200, postToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving post from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<PostDetailDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<CommentVoteDto>> VoteCommentAsync(int commentId, bool? isUpVote)
        {
            try
            {
                var existingComment = _repositoryManager.Comment.GetComment(commentId);
                if (existingComment is null)
                    return ApiResponse<CommentVoteDto>.Failure(404, "Comment does not exist");
               

                var currentApplicationUserId = GetCurrentApplicationUserId();
                if (string.IsNullOrEmpty(currentApplicationUserId))
                    return ApiResponse<CommentVoteDto>.Failure(401, "User not authenticated");
                _logger.LogInformation("User {UserId} voting on comment {CommentId}. Vote type: {VoteType}",
                currentApplicationUserId, commentId, isUpVote.HasValue ? (isUpVote.Value ? "Upvote" : "Downvote") : "Removing vote");
                var existingVote = await _repositoryManager.CommentVote.GetCommentVoteForCommentAsync(commentId, currentApplicationUserId);

                
                if (existingVote == null && isUpVote.HasValue)
                {
                    var newVote = new CommentVote
                    {
                        ApplicationUserId = currentApplicationUserId,
                        CommentId = commentId,
                        IsUpVote = isUpVote.Value
                    };
                    _repositoryManager.CommentVote.AddCommentVote(newVote);
                }
                else if (existingVote != null)
                {
                    if(!isUpVote.HasValue || existingVote.IsUpVote == isUpVote.Value)
                    {
                        _repositoryManager.CommentVote.DeleteCommentVote(existingVote);
                    }
                    else
                    {
                        // Update vote
                        existingVote.IsUpVote = isUpVote;
                        _repositoryManager.CommentVote.UpdateCommentVote(existingVote);
                    }
                }
               
                await _repositoryManager.SaveAsync();
                var (upVoteCount, downVoteCount) = await _repositoryManager.CommentVote.GetCommentVoteCountsAsync(commentId);
                var voteDto = new CommentVoteDto
                {
                    CommentId = commentId,
                    IsUpVote = isUpVote,
                    UpvoteCount = upVoteCount,
                    DownvoteCount = downVoteCount
                };
                
                return ApiResponse<CommentVoteDto>.Success(200, voteDto, "Vote recorded successfully");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while attempting to vote on a comment: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<CommentVoteDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>> GetAllCommentsForPostAsync(int postId, CommentParameters commentParameters)
        {
            try
            {
                if (!commentParameters.ValidDateRange)
                {
                    return ApiResponse<(IEnumerable<CommentDto>, MetaData metaData)>.Failure(400, "End date cannot be less than start date");
                }
                var post = _repositoryManager.Post.PostExists(postId);
                if (!post)
                    return ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>.Failure(404, "Post does not exist.");

                var commentsWithMetadata = await _repositoryManager.Comment.GetCommentsForPostAsync(postId, commentParameters);

                _logger.Log(LogLevel.Information, "Total comments retrieved from the database for post '{id}' is: {count}", postId, commentsWithMetadata.Count());
                
                var commentsDto = commentsWithMetadata.Select(c => c.ToCommentDto()).ToList();
                return ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>.Success(200, (comments: commentsDto, metaData: commentsWithMetadata.MetaData), "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving all comments for a post from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<(IEnumerable<CommentDto> comments, MetaData metaData)>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<CommentDto>> GetCommentForPostAsync(int commentId)
        {
            try
            {
                var cacheKey = $"{GetCommentCacheKey(commentId)}";
                var comment = await _redisCacheService.GetCachedDataAsync<Comment>(cacheKey);
                if(comment is null)
                {
                    comment = await _repositoryManager.Comment.GetCommentForPostAsync(commentId);
                    if (comment is null)
                        return ApiResponse<CommentDto>.Failure(404, "Comment does not exist");
                    else
                    {
                        await _redisCacheService.SetCachedDataAsync<Comment>(cacheKey, comment, TimeSpan.FromMinutes(1));
                    }
                }
                var commentToReturn = comment.ToCommentDto();
                _logger.Log(LogLevel.Information, $"Comment with Id: {commentToReturn.Id} for Post with Id: {comment.PostId} successfully retrieved from the database");
                return ApiResponse<CommentDto>.Success(200, commentToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving a comment for a post from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<CommentDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }

        public async Task<ApiResponse<CommentDto>> CreateCommentForPostAsync(int postId, CreateCommentDto createCommentDto)
        {
            try
            {
                var post = _repositoryManager.Post.PostExists(postId);
                if (!post)
                    return ApiResponse<CommentDto>.Failure(404, "Post does not exist");
                var currentUser = GetCurrentUserName();
                var commentToCreate = new Comment
                {
                    Content = createCommentDto.Content,
                    Author = currentUser ?? "Anonymous",
                    PostId = postId,
                };
                _repositoryManager.Comment.CreateCommentForPost(postId, commentToCreate);
                await _repositoryManager.SaveAsync();
                
                var commentToReturn = commentToCreate.ToCommentDto();
                _logger.Log(LogLevel.Information, "Newly created comment for post '{id}' at {time}", postId, DateTime.Now);
                return ApiResponse<CommentDto>.Success(201, commentToReturn, "Comment created successfully");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while creating a new comment for a post: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<CommentDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<object>> DeleteCommentForPostAsync(int commentId)
        {
            try
            {
                
                var commentToDelete = await _repositoryManager.Comment.GetCommentForPostAsync(commentId);
                if (commentToDelete is null)
                    return ApiResponse<object>.Failure(404, "Comment does not exist");

                // check eligibility to delete comment
                var currentApplicationUserId = GetCurrentApplicationUserId();
                var currentUserRoles = GetCurrentUserRoles();
                var currentUserName = GetCurrentUserName();
                if (currentApplicationUserId != commentToDelete.Post.ApplicationUserId && !currentUserRoles.Contains("Administrator") && currentUserName != commentToDelete.Author)
                    return ApiResponse<object>.Failure(StatusCodes.Status403Forbidden, "You do not have the permission to perform this action");

                _repositoryManager.Comment.DeleteComment(commentToDelete);
                await _repositoryManager.SaveAsync();
                // Invalidate cache
                await _redisCacheService.RemoveCacheAsync(GetCommentCacheKey(commentId));
                _logger.Log(LogLevel.Information, "Comment with Id: '{id}' successfully deleted from the database", commentToDelete.Id);
                return ApiResponse<object>.Success(204, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while deleting a post comment: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<PostDto>> UpdatePostCoverImageAsync(int postId, UpdatePostCoverImageDto updatePostCoverImageDto)
        {
            try
            {
                var postEntity = await _repositoryManager.Post.GetPostAsync(postId);
                if (postEntity == null)
                    return ApiResponse<PostDto>.Failure(404, "Post not found");

                if (updatePostCoverImageDto.PostCoverImage == null)
                    return ApiResponse<PostDto>.Failure(400, "No image file provided");

                // Delete existing image if present
                if (!string.IsNullOrEmpty(postEntity.PostImageUrl))
                {
                    var deleteResult = await _cloudinaryService.DeleteImageAsync(postEntity.ImagePublicId);
                    if (deleteResult.Error != null)
                    {
                        _logger.LogWarning("Failed to delete the previous image with publicId: {PublicId}. Error: {Error}",
                            postEntity.ImagePublicId, deleteResult.Error.Message);
                        // Continue with the upload process even if deletion fails
                    }
                }
                // Upload the new image to cloudinary
                var uploadResult = await _cloudinaryService.UploadImage(updatePostCoverImageDto.PostCoverImage);
                if (uploadResult == null || string.IsNullOrEmpty(uploadResult.Url?.AbsoluteUri))
                {
                    _logger.LogWarning("Image upload operation failed. Result: {Result}", uploadResult?.JsonObj);
                    return ApiResponse<PostDto>.Failure(500, "Image upload failed");
                }

                _logger.LogInformation("New image uploaded successfully. Result: {Result}", uploadResult.JsonObj);

                // Update post entity with new image details
                postEntity.PostImageUrl = uploadResult.Url.AbsoluteUri;
                postEntity.ImagePublicId = uploadResult.PublicId;
                postEntity.ImageFormat = uploadResult.Format;

                // Save changes to the database
                await _repositoryManager.SaveAsync();
                var updatedPost = postEntity.ToPostDto();
                return ApiResponse<PostDto>.Success(200, updatedPost, "Post cover image updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating post cover image for post ID: {PostId}", postId);
                return ApiResponse<PostDto>.Failure(500, "An error occurred while updating the post cover image.");
            }
        }

        public async Task<ApiResponse<CommentDto>> UpdateCommentForPostAsync(int commentId, UpdateCommentDto updateCommentDto)
        {
            try
            {
                var commentEntity = await _repositoryManager.Comment.GetCommentForPostAsync(commentId);
                if (commentEntity is null)
                    return ApiResponse<CommentDto>.Failure(404, "Comment does not exist");

                // check eligibility to delete comment
                var currentApplicationUserId = GetCurrentApplicationUserId();
                var currentUserRoles = GetCurrentUserRoles();
                var currentUserName = GetCurrentUserName();
                if (currentApplicationUserId != commentEntity.Post.ApplicationUserId && !currentUserRoles.Contains("Administrator") && currentUserName != commentEntity.Author)
                    return ApiResponse<CommentDto>.Failure(StatusCodes.Status403Forbidden, "You do not have the permission to perform this action");

                commentEntity.Content = updateCommentDto.Content;

                _repositoryManager.Comment.UpdateCommentForPost(commentEntity);
                await _repositoryManager.SaveAsync();
                // Invalidate cache 
                await _redisCacheService.RemoveCacheAsync(GetCommentCacheKey(commentId));
                _logger.Log(LogLevel.Information, $"Updated comment {commentId} for Post {commentEntity.PostId}. New content: {commentEntity.Content}");
                var updatedCommentDto =  commentEntity.ToCommentDto();
                return ApiResponse<CommentDto>.Success(200, updatedCommentDto, "Comment updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while updating post comment: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<CommentDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }

        public async Task<ApiResponse<object>> UpdatePostAsync(int postId, UpdatePostDto updatePostDto)
        {
            try
            {
                var postEntity = await _repositoryManager.Post.GetPostWithTagsAsync(postId);
                if (postEntity is null)
                    return ApiResponse<object>.Failure(404, "Post does not exist");

                // check eligibility to update a post
                var currentApplicationUserId = GetCurrentApplicationUserId();
                var currentUserRoles = GetCurrentUserRoles();

                if (currentApplicationUserId != postEntity.ApplicationUserId && !currentUserRoles.Contains("Administrator"))
                    return ApiResponse<object>.Failure(StatusCodes.Status403Forbidden, "You do not have the permission to perform this action");

                
                if(!string.IsNullOrWhiteSpace(updatePostDto.Title))
                    postEntity.Title = updatePostDto.Title;
                if(!string.IsNullOrWhiteSpace(updatePostDto.Content))   
                    postEntity.Content = updatePostDto.Content;
                if (updatePostDto.TagsId != null)
                {
                    var uniqueNewTags = updatePostDto.TagsId.Distinct().ToList();
                    var newTags = _repositoryManager.Tag.GetTagsByIds(uniqueNewTags);
                    var newTagsIds =  newTags.Select(t => t.TagId).ToList();
                    // Remove tags that are no longer associated
                    postEntity.TagLinks = postEntity.TagLinks.Where(tl => newTagsIds.Contains(tl.TagId)).ToList();
                   
                    foreach(var tag in newTags)
                    {
                        if(!postEntity.TagLinks.Any(tl=> tl.TagId == tag.TagId))
                        {
                            postEntity.TagLinks.Add(new PostTag { Post=postEntity, TagId=tag.TagId });
                        }
                    }
                }
                postEntity.DateModified = DateTime.Now;
                _repositoryManager.Post.UpdatePost(postEntity);
                await _repositoryManager.SaveAsync();
                // Invalidate cache
                await _redisCacheService.RemoveCacheAsync($"{GetPostCacheKey()}_{postId}");
                return ApiResponse<object>.Success(204, "Post updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while updating post: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<TagDto>> CreateTagAsync(CreateTagDto createTag)
        {
            try
            {
                var currentUserId = GetCurrentApplicationUserId();
                if (currentUserId == null)
                    return ApiResponse<TagDto>.Failure(401, "Unauthorized. Request unsuccessful");
                var user = await _userManager.FindByIdAsync(currentUserId);
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Administrator"))
                    return ApiResponse<TagDto>.Failure(403, "You do not have permission to access this resource");
                var existingTag = _repositoryManager.Tag.GetTagByName(createTag.Name);
                if (existingTag != null)
                    return ApiResponse<TagDto>.Failure(400, "Tag already exists");
                var tagToCreate = new Tag { Name = createTag.Name };
                _repositoryManager.Tag.CreateTag(tagToCreate);
                await _repositoryManager.SaveAsync();
                // Invalidate cache
                await _redisCacheService.RemoveCacheAsync(GetTagCacheKey());

                var tagToReturn = tagToCreate.ToTagDto();
                return ApiResponse<TagDto>.Success(201, tagToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while creating post tag: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<TagDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<object>> UpdateTagAsync(int tagId, UpdateTagDto tagDto)
        {
            try
            {
                var tag = await _repositoryManager.Tag.GetTagAsync(tagId);
                if (tag is null)
                    return ApiResponse<object>.Failure(404, "Tag does not exist");
                
                tag.Name = tagDto.Name;

                _repositoryManager.Tag.UpdateTag(tag);
                await _repositoryManager.SaveAsync();

                // Invalidate cache
                await _redisCacheService.RemoveCacheAsync(GetTagCacheKey());
                return ApiResponse<object>.Success(204, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while updating post tag: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<object>> DeleteTagAsync(int tagId)
        {
            try
            {
                var tag = await _repositoryManager.Tag.GetTagAsync(tagId);
                if (tag is null)
                    return ApiResponse<object>.Failure(404, "Tag does not exist");

                _repositoryManager.Tag.DeleteTag(tag);
                await _repositoryManager.SaveAsync();

                // Invalidate cache
                await _redisCacheService.RemoveCacheAsync(GetTagCacheKey());
                return ApiResponse<object>.Success(204, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while deleting post tag: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<IEnumerable<TagDto>>> GetAllTagsAsync()
        {
            try
            {
                var cacheKey = GetTagCacheKey();

                var tags = await _redisCacheService.GetCachedDataAsync<IEnumerable<Tag>>(cacheKey);
                if (tags is null)
                {
                    tags = await _repositoryManager.Tag.GetAllTagsAsync();
                    await _redisCacheService.SetCachedDataAsync(cacheKey, tags, TimeSpan.FromMinutes(3));
                    _logger.Log(LogLevel.Information, "Tags from database");
                }
                else
                {
                    _logger.Log(LogLevel.Information, "Tags from cache");
                }
                var tagsToReturn = tags.Select(t => t.ToTagDto()).ToList();
                return ApiResponse<IEnumerable<TagDto>>.Success(200, tagsToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving all post tags from the database: {ex.Message}");
               // _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<IEnumerable<TagDto>>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<TagDto>> GetTagByIdAsync(int tagId)
        {
            try
            {
                var cacheKey = $"Blog_Cache_Tags_{tagId}";
                var tag = await _redisCacheService.GetCachedDataAsync<Tag>(cacheKey);
                if(tag is null)
                {
                   tag = await _repositoryManager.Tag.GetTagAsync(tagId);
                    if (tag is null)
                        return ApiResponse<TagDto>.Success(404, "Tag does not exist");
                    await _redisCacheService.SetCachedDataAsync(cacheKey, tag, TimeSpan.FromMinutes(3));
                }
                
                var tagToReturn = tag.ToTagDto();
                return ApiResponse<TagDto>.Success(200, tagToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving post tag from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<TagDto>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<IEnumerable<PostDto>>> GetAllPostsForTagAsync(int tagId)
        {
            try
            {
                var tag = await _repositoryManager.Tag.GetTagAsync(tagId);
                if (tag is null)
                    return ApiResponse<IEnumerable<PostDto>>.Failure(404, "Tag does not exist");
                var posts = await _repositoryManager.Tag.GetAllPostsForTagAsync(tagId);
                var postsToReturn = posts.Select(p=> p.ToPostDto()).ToList();
                return ApiResponse<IEnumerable<PostDto>>.Success(200, postsToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving all posts for a tag from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<IEnumerable<PostDto>>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }
        #region Private methods
        private string? GetCurrentUserName()
        {
            return _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        }
        private string? GetCurrentApplicationUserId()
        {
            return _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        private string? GetCurrentUserEmail()
        {
            return _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        }
        private IEnumerable<string> GetCurrentUserRoles()
        {
            return _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
        }
        private async Task<Post> GetPostFromDb(int postId) => await _repositoryManager.Post.GetPostAsync(postId);
        private string GetTagCacheKey() => "Blog_Cache_Tags";
        private string GetPostCacheKey() => "Blog_Cache_Posts";
        private string GetCommentCacheKey(int commentId) => $"Blog_Cache_Comments_{commentId}";
        #endregion
    }
}
