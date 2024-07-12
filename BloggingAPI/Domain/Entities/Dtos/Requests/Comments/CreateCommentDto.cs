using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Domain.Entities.Dtos.Requests.Comments
{
    public record CreateCommentDto(
    [Required(ErrorMessage ="Content field is required")]
    string Content);
}
