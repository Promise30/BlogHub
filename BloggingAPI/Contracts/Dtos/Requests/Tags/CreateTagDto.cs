using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Contracts.Dtos.Requests.Tags
{
    public class CreateTagDto
    {
        [Required(ErrorMessage = "A tag name is required")]
        public string Name { get; set; }
    }
}