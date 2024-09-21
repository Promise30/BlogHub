using BloggingAPI.Contracts.Dtos.Requests.Comments;
using BloggingAPI.Contracts.Dtos.Requests.Posts;
using BloggingAPI.Contracts.Dtos.Requests.Tags;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Posts;
using BloggingAPI.Persistence.RequestFeatures;
using BloggingAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Text.Json;

namespace BloggingAPI.Presentation.Controllers
{
    [Route("api/posts")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "v1")]
    public class PostsController : ControllerBase
    {
        private readonly IBloggingService _bloggingService;
        public PostsController(IBloggingService bloggingService)
        {
            _bloggingService = bloggingService;
        }
        /// <summary>
        /// Retrieves all posts with pagination and other filtering properties. Only accessible to user with 'Administrator' role
        /// </summary>
        /// <param name="postParameters">The parameters for pagination and filtering.</param>
        /// <returns>A list of posts with pagination metadata.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PostDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPosts([FromQuery] PostParameters postParameters)
        {
            var result = await _bloggingService.GetAllPostsAsync(postParameters);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.Data.metaData));
            return StatusCode(result.StatusCode, result.Data.posts);
        }
        /// <summary>
        /// Retrieves all posts for the authenticated user with pagination and filtering properties.
        /// </summary>
        /// <param name="postParameters">The parameters for pagination and filtering.</param>
        /// <returns>A list of user posts with pagination metadata.</returns>   
        [Authorize]
        [HttpGet("user-posts")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PostDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPostsForUser([FromQuery] PostParameters postParameters)
        {
            var result = await _bloggingService.GetAllUserPostsAsync(postParameters);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.Data.metaData));
            return StatusCode(result.StatusCode, result.Data.posts);
        }
        /// <summary>
        /// Retrieves a post by its ID.
        /// </summary>
        /// <param name="id">The ID of the post.</param>
        /// <returns>The post with the specified ID.</returns>
        [Authorize]
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<PostDetailDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPost(int id)
        {
            var result = await _bloggingService.GetPostAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Creates a new post.
        /// </summary>
        /// <param name="createPostDto">The post data to create.</param>
        /// <returns>The created post.</returns>
        [Authorize]
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<NewPostDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]

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
        /// <summary>
        /// Updates an existing post.
        /// </summary>
        /// <param name="postId">The ID of the post to update.</param>
        /// <param name="updatePostDto">The updated post data.</param>
        /// <returns>The updated post.</returns>
        [Authorize]
        [HttpPatch("{postId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePost(int postId, UpdatePostDto updatePostDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _bloggingService.UpdatePostAsync(postId, updatePostDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Updates the cover image of an existing post.
        /// </summary>
        /// <param name="postId">The ID of the post to update.</param>
        /// <param name="updatePostCoverImage">The updated cover image data.</param>
        /// <returns>The updated post with the new cover</returns>  
        [Authorize]
        [HttpPatch("{postId:int}/cover-image")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<PostDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePostCoverImage(int postId, UpdatePostCoverImageDto updatePostCoverImage)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }
            var result = await _bloggingService.UpdatePostCoverImageAsync(postId, updatePostCoverImage);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Deletes a post by its ID.
        /// </summary>
        /// <param name="id">The ID of the post to delete.</param>
        /// <returns>A status indicating the result of the operation.</returns> 
        [Authorize]
        [HttpDelete("{id:int}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePost(int id)
        {
            var result = await _bloggingService.DeletePostAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        
    }
}
