using BloggingAPI.Domain.Enums;
using BloggingAPI.Contracts.Validations;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Posts
{
    public record UpdatePostDto
    {
        [Required(ErrorMessage = "Title field is required")]
        public string Title { get; init; }
        [Required(ErrorMessage = "Content field is required")]
        public string Content { get; init; }
        [Required(ErrorMessage = "Category field is required")]
        [ValidPostCategory(ErrorMessage = "Invalid post category type")]
        public string Category { get; set; }
        [AllowedFileExtensions(new[] { ".jpg", ".png", ".jpeg" }, ErrorMessage = "Only JPG, JPEG and PNG files are allowed")]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = "File size must not exceed 2 MB")]
        public IFormFile? PostCoverImage { get; init; }
    }

}
