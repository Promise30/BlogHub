using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Comments
{
    public record UpdateCommentDto
    (
        [Required(ErrorMessage ="Content field is required")]
        string Content
    );
}
