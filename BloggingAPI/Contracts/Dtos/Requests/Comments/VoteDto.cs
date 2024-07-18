using System.Text.Json.Serialization;

namespace BloggingAPI.Contracts.Dtos.Requests.Comments
{
    public class VoteDto
    {
        [JsonPropertyName("IsUpVote")]
        public bool? IsUpVote { get; set; }
    }
}
