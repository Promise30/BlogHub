using BloggingAPI.Contracts.Dtos.Requests.Comments;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Comments;
using BloggingAPI.Persistence.RequestFeatures;
using BloggingAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace BloggingAPI.Presentation.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly IBloggingService _bloggingService;
        public CommentsController(IBloggingService bloggingService)
        {
            _bloggingService = bloggingService;
        }
        /// <summary>
        /// Retrieves all comments for a specific post with pagination.
        /// </summary>
        /// <param name="id">The ID of the post.</param>
        /// <param name="commentParameters">The parameters for pagination and filtering.</param>
        /// <returns>A list of comments with pagination</returns>   
        [AllowAnonymous]
        [HttpGet("posts/{id:int}")]
        [Produces("application/json")]  
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCommentsForPost(int id, [FromQuery] CommentParameters commentParameters)
        {
            var result = await _bloggingService.GetAllCommentsForPostAsync(id, commentParameters);
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(result.Data.metaData));
            return StatusCode(result.StatusCode, result.Data.comments);
        }
        /// <summary>
        /// Retrieves a specific comment by its ID.
        /// </summary>
        /// <param name="commentId">The ID of the comment.</param>
        /// <returns>The comment with the specified ID</returns>
        [AllowAnonymous]
        [HttpGet("{commentId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCommentForPost(int commentId)
        {
            var result = await _bloggingService.GetCommentForPostAsync(commentId);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Creates a new comment for a specific post.
        /// </summary>
        /// <param name="postId">The ID of the post.</param>
        /// <param name="createCommentDto">The comment data to create.</param>
        /// <returns>The created comment.</returns>
        [AllowAnonymous]
        [HttpPost("posts/{postId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCommentForPost(int postId, [FromBody] CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _bloggingService.CreateCommentForPostAsync(postId, createCommentDto);
            if(result.StatusCode == 201)
                return CreatedAtAction(nameof(GetCommentForPost), new { postId, commentId = result.Data.Id }, result);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Updates an existing comment.
        /// </summary>
        /// <param name="commentId">The ID of the comment to update.</param>
        /// <param name="updateCommentDto">The updated comment data.</param>
        /// <returns>The updated comment.</returns>
        [Authorize]
        [HttpPut("{commentId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCommentForPost(int commentId, [FromBody] UpdateCommentDto updateCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _bloggingService.UpdateCommentForPostAsync(commentId, updateCommentDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Deletes a comment by its ID.
        /// </summary>
        /// <param name="commentId">The ID of the comment to delete.</param>
        /// <returns>A status indicating the result of</returns>
        [Authorize]
        [HttpDelete("{commentId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]    
        public async Task<IActionResult> DeleteCommentForPost(int commentId)
        {
            var result = await _bloggingService.DeleteCommentForPostAsync(commentId);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Votes on a comment (upvote or downvote).
        /// </summary>
        /// <param name="commentId">The ID of the comment to vote on.</param>
        /// <param name="votePayload">The vote data indicating upvote or downvote.</param>
        /// <returns>The result of the vote operation.</returns>
        [Authorize]
        [HttpPost("{commentId:int}/vote")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<CommentVoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VoteComment(int commentId, [FromBody] VoteDto votePayload)
        {
            var result = await _bloggingService.VoteCommentAsync(commentId, votePayload.IsUpVote);
            return StatusCode(result.StatusCode, result);
        }

    }
}
