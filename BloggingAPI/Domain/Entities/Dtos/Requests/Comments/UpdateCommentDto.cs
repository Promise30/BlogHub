using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities.Dtos.Requests.Comments
{
    public record UpdateCommentDto
    (
        [Required(ErrorMessage ="Content field is required")]
        string Content
    );
}
