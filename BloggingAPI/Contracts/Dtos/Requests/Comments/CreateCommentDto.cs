using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Comments
{
    public record CreateCommentDto(
    [Required(ErrorMessage ="Content field is required")]
    string Content);
}
