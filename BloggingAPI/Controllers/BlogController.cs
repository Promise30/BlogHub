using BloggingAPI.Domain.Entities.Dtos.Requests.Comments;
using BloggingAPI.Domain.Entities.Dtos.Requests.Posts;
using BloggingAPI.Domain.Entities.Dtos.Responses;
using BloggingAPI.Domain.Entities.RequestFeatures;
using BloggingAPI.Extensions;
using BloggingAPI.Infrastructure.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Text.Json;

namespace BloggingAPI.Controllers
{
    [Route("api/blogs")]
    [ApiController]
    //[Authorize]
    //[ApiExplorerSettings(GroupName = "v1")]
    public class BlogController : ControllerBase
    {
        private readonly IBloggingService _bloggingService;
        public BlogController(IBloggingService bloggingService)
        {
            _bloggingService = bloggingService;
        }
       
        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts([FromQuery]PostParameters postParameters)
        {
            var result = await _bloggingService.GetAllPostsAsync(postParameters);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.Data.metaData));
            return StatusCode(result.StatusCode, result.Data.posts);
        }
        [HttpGet("posts/{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var result = await _bloggingService.GetPostonlyAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("posts/postWithComments/{postId}")]
        public async Task<IActionResult> GetPostWithComments(int postId)
        {
            var result = await _bloggingService.GetPostWithCommentsAsync(postId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("posts")]
        public async Task<IActionResult> CreatePost(CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _bloggingService.CreatePostAsync(createPostDto);
            return CreatedAtAction(nameof(GetPost), new { id = result.Data.Id }, result.Data);
        }
        [HttpPut("posts/{postId:int}")]
        public async Task<IActionResult> UpdatePost(int postId, UpdatePostDto updatePostDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _bloggingService.UpdatePostAsync(postId, updatePostDto);
            return StatusCode(result.StatusCode);
        }
        [HttpDelete("posts/{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var result = await _bloggingService.DeletePostAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        // Commment Related Routes
        [HttpGet("posts/{id:int}/comments")]
        public async Task<IActionResult> GetCommentsForPost(int id, [FromQuery] CommentParameters commentParameters)
        {
            var result = await _bloggingService.GetAllCommentsForPostAsync(id, commentParameters);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.Data.metaData));
            return StatusCode(result.StatusCode, result.Data.comments);
        }
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
            return CreatedAtAction(nameof(GetCommentForPost), new { postId = postId, commentId = result.Data.Id }, result.Data);
        }
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
        [HttpDelete("posts/{postId:int}/comments/{commentId:int}")]
        public async Task<IActionResult> DeleteCommentForPost(int postId, int commentId)
        {
            var result = await _bloggingService.DeleteCommentForPostAsync(postId, commentId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
