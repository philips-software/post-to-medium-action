
using System.Text.Json.Serialization;

namespace PostMediumGitHubAction
{
    public class Data
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class MediumUser
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }
}