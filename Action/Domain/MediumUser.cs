using System.Text.Json.Serialization;

namespace PostMediumGitHubAction.Domain
{
    public class User
    {
        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("username")] public string Username { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("url")] public string Url { get; set; }

        [JsonPropertyName("imageUrl")] public string ImageUrl { get; set; }
    }
}