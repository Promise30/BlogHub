using BloggingAPI.Domain.Enums;
using BloggingAPI.Contracts.Validations;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Posts
{
    public record UpdatePostDto
    { 
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<int> TagsId { get; set; } = [];
    }

}
