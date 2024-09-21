using BloggingAPI.Contracts.Dtos.Requests.Tags;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Posts;
using BloggingAPI.Contracts.Dtos.Responses.Tags;
using BloggingAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloggingAPI.Presentation.Controllers
{
    [Route("api/tags")]
    [ApiController]
    public class TagsController(IBloggingService bloggingService) : ControllerBase
    {
        private readonly IBloggingService _bloggingService = bloggingService;
        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <returns>A list of tags.</returns>
        /// <response code="200">Returns a list of tags.</response>
        /// <response code="500">Returns an error message if an unexpected error occurs.</response>

        [AllowAnonymous]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TagDto>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTags()
        {
            var result = await _bloggingService.GetAllTagsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieves a tag by its ID.
        /// </summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <returns>The tag with the specified ID.</returns>
        [AllowAnonymous]
        [HttpGet("{tagId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<TagDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTagById(int tagId)
        {
            var result = await _bloggingService.GetTagByIdAsync(tagId);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Retrieves all posts associated with a specific tag.
        /// </summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <returns>A list of posts associated with the specified tag.</returns>
        [AllowAnonymous]
        [HttpGet("{tagId:int}/posts")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PostDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPostsForTag(int tagId)
        {
            var result = await _bloggingService.GetAllPostsForTagAsync(tagId);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Creates a new tag. Only available to user with 'Administrator' role.
        /// </summary>
        /// <param name="createTag">The tag data to create.</param>
        /// <returns>The created tag.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>),StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTag(CreateTagDto createTag)
        {
            var result = await _bloggingService.CreateTagAsync(createTag);
            if(result.StatusCode == 201)
                return CreatedAtAction(nameof(GetTagById), new { tagId = result.Data.Id }, result.Data);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Updates an existing tag. Only available to user with 'Administrator' role.
        /// </summary>
        /// <param name="tagId">The ID of the tag to update.</param>
        /// <param name="tagDto">The updated tag data.</param>
        /// <returns>The updated tag.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut("{tagId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTag(int tagId, UpdateTagDto tagDto)
        {
            var result = await _bloggingService.UpdateTagAsync(tagId, tagDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Deletes a tag by its ID. Only accessible to user with 'Administrator' role.
        /// </summary>
        /// <param name="tagId">The ID of the tag to delete.</param>
        /// <returns>A status indicating the result of the operation</returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{tagId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTag(int tagId)
        {
            var result = await _bloggingService.DeleteTagAsync(tagId);
            return StatusCode(result.StatusCode, result);

        }

    }

}
