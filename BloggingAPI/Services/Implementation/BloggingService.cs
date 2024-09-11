using BloggingAPI.Contracts.Dtos.Requests.Comments;
using BloggingAPI.Contracts.Dtos.Requests.Posts;
using BloggingAPI.Contracts.Dtos.Requests.Tags;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Posts;
using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Repositories;
using BloggingAPI.Persistence.Extensions;
using BloggingAPI.Persistence.RequestFeatures;
using BloggingAPI.Services.Interface;
using CloudinaryDotNet.Actions;
using Hangfire;
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
        private bool _isFromCache = false;
        public BloggingService(ILogger<BloggingService> logger,
                               IRepositoryManager repositoryManager,
                               IHttpContextAccessor httpContextAccessor,
                               IEmailService emailService,
                               ICloudinaryService cloudinaryService,
                               IUrlHelper urlHelper,
                               IEmailTemplateService emailTemplateService,
                               IRedisCacheService redisCacheService)
        {
            _logger = logger;
            _repositoryManager = repositoryManager;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _cloudinaryService = cloudinaryService;
            _urlHelper = urlHelper;
            _emailTemplateService = emailTemplateService;
            _redisCacheService = redisCacheService;
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
                var cacheKey = $"posts:{postParameters.PageNumber}:{postParameters.PageSize}:{postParameters.OrderBy}:{postParameters.SearchTerm}:{postParameters.Tag}:{postParameters.StartDate}:{postParameters.EndDate}";
                var cachedData = _redisCacheService.GetCachedData<(List<PostDto> posts, MetaData metaData)>(cacheKey);

                if (cachedData.posts == null)
                {
                    var postsWithMetaData = await _repositoryManager.Post.GetAllPostsAsync(postParameters);
                    _logger.Log(LogLevel.Information, "Total posts retrieved from the database: {count}", postsWithMetaData.Count());

                    // Convert to PostDto and cache only the necessary parts
                    var postsDto = postsWithMetaData.Select(p => p.ToPostDto()).ToList();
                    _redisCacheService.SetCachedData(cacheKey, (postsDto, postsWithMetaData.MetaData), TimeSpan.FromMinutes(3));

                    return ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>.Success(200, (postsDto, postsWithMetaData.MetaData), "Request successful");
                }
                else
                {
                    _logger.Log(LogLevel.Information, "Posts were retrieved from cache");
                    return ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>.Success(200, (cachedData.posts, cachedData.metaData), "Request successful");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving posts from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<(IEnumerable<PostDto>, MetaData metaData)>.Failure(500, "An error occurred. Request unsuccessful.");
            }
        }

        //public async Task<ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>> GetAllPostsAsync(PostParameters postParameters)
        //{
        //    try
        //    {
        //        var cacheKey = $"posts:{postParameters.PageNumber}:{postParameters.PageSize}:{postParameters.OrderBy}:{postParameters.SearchTerm}:{postParameters.Tag}:{postParameters.StartDate}:{postParameters.EndDate}";
        //        var postsWithMetaData = _redisCacheService.GetCachedData<PagedList<Post>>(cacheKey);
        //        if (postsWithMetaData is null)
        //        {
        //            postsWithMetaData = await _repositoryManager.Post.GetAllPostsAsync(postParameters);
        //            _logger.Log(LogLevel.Information, "Total posts retrieved from the database: {count}", postsWithMetaData.Count());
        //            _redisCacheService.SetCachedData(cacheKey, postsWithMetaData, TimeSpan.FromMinutes(3));
        //        }
        //        else
        //            _logger.Log(LogLevel.Information, "Posts were retrieved from cache");
        //        var postsDto = postsWithMetaData.Select(p=> p.ToPostDto()).ToList();
        //        return ApiResponse<(IEnumerable<PostDto> posts, MetaData metaData)>.Success(200, (posts: postsDto, metaData: postsWithMetaData.MetaData), "Request successful");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Log(LogLevel.Error, $"Error occurred while retrieving posts from the database: {ex.Message}");
        //        _logger.Log(LogLevel.Error, ex.StackTrace);
        //        return ApiResponse<(IEnumerable<PostDto>, MetaData metaData)>.Failure(500, "An error occurred. Request unsuccessful.");
        //    }
        //}
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
                var post = _redisCacheService.GetCachedData<Post>(cacheKey);
                if (post is null)
                {
                    post = await GetPostFromDb(postId);
                    if (post is null)
                        return ApiResponse<PostDetailDto>.Failure(404, "Post not found");
                    _logger.Log(LogLevel.Information, "Post successfully retrieved from the database");
                    _redisCacheService.SetCachedData(cacheKey, post, TimeSpan.FromMinutes(3));
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
        public async Task<ApiResponse<object>> VoteCommentAsync(int postId, int commentId, bool? isUpVote)
        {
            try
            {
                _logger.Log(LogLevel.Information, $"User's vote option: {isUpVote}");
                var currentApplicationUserId = GetCurrentApplicationUserId();
                var existingVote = await _repositoryManager.CommentVote.GetCommentVoteForCommentAsync(commentId, currentApplicationUserId);
                if (existingVote == null)
                {
                    var newCommentVote = new CommentVote
                    {
                        ApplicationUserId = currentApplicationUserId,
                        CommentId = commentId,
                        IsUpVote = isUpVote
                    };
                    _repositoryManager.CommentVote.AddCommentVote(newCommentVote);
                }
                else if (existingVote.IsUpVote != isUpVote)
                {
                    existingVote.IsUpVote = isUpVote;
                    _repositoryManager.CommentVote.UpdateCommentVote(existingVote);
                }
                else
                {
                    _repositoryManager.CommentVote.DeleteCommentVote(existingVote);
                }
                await _repositoryManager.SaveAsync();
                return ApiResponse<object>.Success(204, null, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while attempting to vote on a comment: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
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
                //var comments = post.Comment.ToList();
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
        public async Task<ApiResponse<CommentDto>> GetCommentForPostAsync(int postId, int commentId)
        {
            try
            {
                var cacheKey = $"{GetPostCacheKey()}_{postId}_{GetCommentCacheKey()}_{commentId}";
                var post = _repositoryManager.Post.PostExists(postId);
                if (!post)
                    return ApiResponse<CommentDto>.Failure(404, "Post does not exist");
                var comment = await _repositoryManager.Comment.GetCommentForPostAsync(postId, commentId);
                if (comment is null)
                    return ApiResponse<CommentDto>.Failure(404, "Comment does not exist");
                var commentToReturn = comment.ToCommentDto();
                _logger.Log(LogLevel.Information, $"Comment with Id: {commentToReturn.Id} for Post with Id: {postId} successfully retrieved from the database");
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
                var commentToCreate = new Comment
                {
                    Content = createCommentDto.Content,
                    Author = GetCurrentUserName() ?? "Anonymous",
                    PostId = postId,
                   // ApplicationUserId = GetCurrentApplicationUserId() ?? string.Empty,
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
        public async Task<ApiResponse<object>> DeleteCommentForPostAsync(int postId, int commentId)
        {
            try
            {
                var post = await GetPostFromDb(postId);
                if (post is null)
                    return ApiResponse<object>.Failure(404, "Post does not exist");
                var commentToDelete = await _repositoryManager.Comment.GetCommentForPostAsync(postId, commentId);
                if (commentToDelete is null)
                    return ApiResponse<object>.Failure(404, "Comment does not exist");

                // check eligibility to delete comment
                var currentApplicationUserId = GetCurrentApplicationUserId();
                var currentUserRoles = GetCurrentUserRoles();
                var currentUserName = GetCurrentUserName();
                if (currentApplicationUserId != post.ApplicationUserId || !currentUserRoles.Contains("Administrator") || currentUserName != commentToDelete.Author)
                    return ApiResponse<object>.Failure(StatusCodes.Status403Forbidden, "You do not have the permission to perform this action");

                _repositoryManager.Comment.DeleteComment(commentToDelete);
                await _repositoryManager.SaveAsync();
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
        public async Task<ApiResponse<object>> UpdatePostCoverImageAsync(int postId, UpdatePostCoverImageDto updatePostCoverImageDto)
        {
            try
            {
                var postEntity = await _repositoryManager.Post.GetPostAsync(postId);
                if (postEntity == null)
                    return ApiResponse<object>.Failure(404, "Post not found");
                if (!string.IsNullOrEmpty(postEntity?.PostImageUrl))
                {
                    var deleteResult = await _cloudinaryService.DeleteImageAsync(postEntity.ImagePublicId);
                    if (deleteResult.Result != null)
                    {
                        _logger.Log(LogLevel.Warning, "Failed to delete the previous image with publicId: {0}", postEntity.ImagePublicId);
                        return ApiResponse<object>.Failure(500, "Failed to delete the existing image");
                    }
                        // upload the new image to cloudinary
                    var uploadResult = await _cloudinaryService.UploadImage(updatePostCoverImageDto.PostCoverImage);
                    if (uploadResult == null || string.IsNullOrEmpty(uploadResult?.Url.AbsoluteUri))
                    {
                        _logger.Log(LogLevel.Warning, $"Image upload operation failed, result: {uploadResult?.JsonObj}");
                        return ApiResponse<object>.Failure(500, "Image upload failed");
                    }

                    _logger.Log(LogLevel.Information, $"Result of the new image upload: {uploadResult?.JsonObj}");
                    postEntity.PostImageUrl = uploadResult?.Url.AbsoluteUri;
                    postEntity.ImagePublicId = uploadResult?.PublicId;
                    postEntity.ImageFormat = uploadResult?.Format;
                }
                await _repositoryManager.SaveAsync();
                return ApiResponse<object>.Success(204, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while updating post cover image: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
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

                // check eligibility to delete comment
                var currentApplicationUserId = GetCurrentApplicationUserId();
                var currentUserRoles = GetCurrentUserRoles();
                var currentUserName = GetCurrentUserName();
                if (currentApplicationUserId != post.ApplicationUserId || !currentUserRoles.Contains("Administrator") || currentUserName != commentEntity.Author)
                    return ApiResponse<object>.Failure(StatusCodes.Status403Forbidden, "You do not have the permission to perform this action");

                commentEntity.Content = updateCommentDto.Content;

                _repositoryManager.Comment.UpdateCommentForPost(commentEntity);
                await _repositoryManager.SaveAsync();
                _logger.Log(LogLevel.Information, $"Newly updated comment for Post with Id: {post.PostId} is: {JsonSerializer.Serialize(commentEntity)}");
                return ApiResponse<object>.Success(204, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while updating post comment: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<object>.Failure(500, "An error occurred. Request unsuccessful.");
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

                if (currentApplicationUserId != postEntity.ApplicationUserId || !currentUserRoles.Contains("Administrator"))
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
                await _repositoryManager.SaveAsync();
                var updatedPostDto = postEntity.ToPostDto();
                return ApiResponse<object>.Success(200, updatedPostDto, "Post updated successfully");
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
                var existingTag = _repositoryManager.Tag.GetTagByName(createTag.Name);
                if (existingTag != null)
                    return ApiResponse<TagDto>.Failure(400, "Tag already exists");
                var tagToCreate = new Tag { Name = createTag.Name };
                _repositoryManager.Tag.CreateTag(tagToCreate);
                await _repositoryManager.SaveAsync();
                // Invalidate cache
                _redisCacheService.RemoveCache(GetTagCacheKey());

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
                _redisCacheService.RemoveCache(GetTagCacheKey());
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
                _redisCacheService.RemoveCache(GetTagCacheKey());
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

                var tags = _redisCacheService.GetCachedData<IEnumerable<Tag>>(cacheKey);
                if (tags is null)
                {
                    tags = await _repositoryManager.Tag.GetAllTagsAsync();
                    _redisCacheService.SetCachedData(cacheKey, tags, TimeSpan.FromMinutes(3));
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
                var tag = _redisCacheService.GetCachedData<Tag>(cacheKey);
                if(tag is null)
                {
                   tag = await _repositoryManager.Tag.GetTagAsync(tagId);
                    if (tag is null)
                        return ApiResponse<TagDto>.Success(404, "Tag does not exist");
                    _redisCacheService.SetCachedData(cacheKey, tag, TimeSpan.FromMinutes(3));
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
        public async Task<ApiResponse<IEnumerable<PostDetailDto>>> GetAllPostsForTagAsync(int tagId)
        {
            try
            {
                var tag = await _repositoryManager.Tag.GetTagAsync(tagId);
                if (tag is null)
                    return ApiResponse<IEnumerable<PostDetailDto>>.Failure(404, "Tag does not exist");
                var posts = await _repositoryManager.Tag.GetAllPostsForTagAsync(tagId);
                var postsToReturn = posts.Select(p=> p.ToPostDetailDto()).ToList();
                return ApiResponse<IEnumerable<PostDetailDto>>.Success(200, postsToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error occurred while retrieving all posts for a tag from the database: {ex.Message}");
                _logger.Log(LogLevel.Error, ex.StackTrace);
                return ApiResponse<IEnumerable<PostDetailDto>>.Failure(500, "An error occurred. Request unsuccessful.");
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
        private string GetCommentCacheKey() => "Blog_Cache_Comments";
        #endregion
    }
}
