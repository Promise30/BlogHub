using System.Text.Json.Serialization;

namespace BloggingAPI.Domain.Entities
{
    public class VotePayload
    {
        [JsonPropertyName("IsUpVote")]
        public bool? IsUpVote { get; set; }
    }
}
