using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Comments
{
    public class UpdateCommentDto
    {
        [Required(ErrorMessage = "Content field is required")]
        public string Content { get; set; } = string.Empty;
    }
}
