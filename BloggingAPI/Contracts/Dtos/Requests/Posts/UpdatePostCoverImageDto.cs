using BloggingAPI.Contracts.Validations;

namespace BloggingAPI.Contracts.Dtos.Requests.Posts
{
    public class UpdatePostCoverImageDto
    {

        [AllowedFileExtensions(new[] { ".jpg", ".png", ".jpeg" }, ErrorMessage = "Only JPG, JPEG and PNG files are allowed")]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = "File size must not exceed 2 MB")]
        public IFormFile? PostCoverImage { get; set; }
    }
}
