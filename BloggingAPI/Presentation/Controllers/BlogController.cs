using BloggingAPI.Contracts.Dtos.Requests.Comments;
using BloggingAPI.Contracts.Dtos.Requests.Posts;
using BloggingAPI.Contracts.Dtos.Requests.Tags;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Persistence.RequestFeatures;
using BloggingAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Text.Json;

namespace BloggingAPI.Presentation.Controllers
{
    [Route("api/blogs")]
    [ApiController]
    ////[Authorize]
    //[ApiExplorerSettings(GroupName = "v1")]
    public class BlogController : ControllerBase
    {
        private readonly IBloggingService _bloggingService;
        public BlogController(IBloggingService bloggingService)
        {
            _bloggingService = bloggingService;
        }
        //[Authorize(Roles = "Administrator")]
        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts([FromQuery] PostParameters postParameters)
        {
            var result = await _bloggingService.GetAllPostsAsync(postParameters);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.Data.metaData));
            return StatusCode(result.StatusCode, result.Data.posts);
        }
        ////[Authorize]
        [HttpGet("user-posts")]
        public async Task<IActionResult> GetPostsForUser([FromQuery] PostParameters postParameters)
        {
            var result = await _bloggingService.GetAllUserPostsAsync(postParameters);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.Data.metaData));
            return StatusCode(result.StatusCode, result.Data.posts);
        }
        ////[Authorize]
        [HttpGet("posts/{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var result = await _bloggingService.GetPostAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        ////[Authorize]
        [HttpPost("posts")]
        public async Task<IActionResult> CreatePost(CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _bloggingService.CreatePostAsync(createPostDto);
            if(result.StatusCode == 201)
                return CreatedAtAction(nameof(GetPost), new { id = result.Data.Id }, result.Data);
            return StatusCode(result.StatusCode, result);
        }
        
        ////[Authorize]
        [HttpPatch("posts/{postId:int}")]
        public async Task<IActionResult> UpdatePost(int postId, UpdatePostDto updatePostDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
                //return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _bloggingService.UpdatePostAsync(postId, updatePostDto);
            return StatusCode(result.StatusCode, result);
        }
        ////[Authorize]
        [HttpPatch("posts/{postId:int}/cover-image")]
        public async Task<IActionResult> UpdatePostCoverImage(int postId, UpdatePostCoverImageDto updatePostCoverImage)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }
            var result = await _bloggingService.UpdatePostCoverImageAsync(postId, updatePostCoverImage);
            return StatusCode(result.StatusCode, result);
        }

        ////[Authorize]
        [HttpDelete("posts/{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var result = await _bloggingService.DeletePostAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        // Commment Related Routes
        [AllowAnonymous]
        [HttpGet("posts/{id:int}/comments")]
        public async Task<IActionResult> GetCommentsForPost(int id, [FromQuery] CommentParameters commentParameters)
        {
            var result = await _bloggingService.GetAllCommentsForPostAsync(id, commentParameters);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.Data.metaData));
            return StatusCode(result.StatusCode, result.Data.comments);
        }
        [AllowAnonymous]
        [HttpGet("posts/{postId:int}/comments/{commentId:int}")]
        public async Task<IActionResult> GetCommentForPost(int postId, int commentId)
        {
            var result = await _bloggingService.GetCommentForPostAsync(postId, commentId);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpPost("posts/{postId:int}/comments")]
        public async Task<IActionResult> CreateCommentForPost(int postId, [FromBody] CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _bloggingService.CreateCommentForPostAsync(postId, createCommentDto);
            return CreatedAtAction(nameof(GetCommentForPost), new { postId, commentId = result.Data.Id }, result.Data);
        }
        [AllowAnonymous]
        [HttpPut("posts/{postId:int}/comments/{commentId:int}")]
        public async Task<IActionResult> UpdateCommentForPost(int postId, int commentId, [FromBody] UpdateCommentDto updateCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _bloggingService.UpdateCommentForPostAsync(postId, commentId, updateCommentDto);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpDelete("posts/{postId:int}/comments/{commentId:int}")]
        public async Task<IActionResult> DeleteCommentForPost(int postId, int commentId)
        {
            var result = await _bloggingService.DeleteCommentForPostAsync(postId, commentId);
            return StatusCode(result.StatusCode, result);
        }
        //[Authorize]
        [HttpPost("posts/{postId:int}/comments/{commentId:int}/vote")]
        public async Task<IActionResult> VoteComment(int postId, int commentId, [FromBody] VoteDto votePayload)
        {
            var result = await _bloggingService.VoteCommentAsync(postId, commentId, votePayload.IsUpVote);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpGet("tags")]
        public async Task<IActionResult> GetAllTags()
        {
            var result = await _bloggingService.GetAllTagsAsync();
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpGet("tags/{tagId:int}")]
        public async Task<IActionResult> GetTagById(int tagId)
        {
            var result = await _bloggingService.GetTagByIdAsync(tagId);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpGet("tags/{tagId:int}/posts")]
        public async Task<IActionResult> GetPostsForTag(int tagId)
        {
            var result = await _bloggingService.GetAllPostsForTagAsync(tagId);
            return StatusCode(result.StatusCode, result);
        }
        //[Authorize]
        [HttpPost("tags")]
        public async Task<IActionResult> CreateTag(CreateTagDto createTag)
        {
            var result = await _bloggingService.CreateTagAsync(createTag);
            return CreatedAtAction(nameof(GetTagById), new { tagId = result.Data.Id }, result.Data);
        }
        //[Authorize]
        [HttpPut("tags/{tagId:int}")]
        public async Task<IActionResult> UpdateTag(int tagId, UpdateTagDto tagDto)
        {
            var result = await _bloggingService.UpdateTagAsync(tagId, tagDto);
            return StatusCode(result.StatusCode, result);
        }
        //[Authorize]
        [HttpDelete("tags/{tagId:int}")]
        public async Task<IActionResult> DeleteTag(int tagId)
        {
            var result = await _bloggingService.DeleteTagAsync(tagId);
            return StatusCode(result.StatusCode, result);

        }

    }
}
