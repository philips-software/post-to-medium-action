
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PostMediumGitHubAction
{
    public class Publication
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class MediumPublication
    {
        [JsonPropertyName("data")]
        public List<Publication> Publications { get; set; }
    }
}