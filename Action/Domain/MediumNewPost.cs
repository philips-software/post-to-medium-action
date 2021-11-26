using System.Text.Json.Serialization;

namespace PostMediumGitHubAction.Domain
{
    public class Post
    {
        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("tags")] public string[] Tags { get; set; }

        [JsonPropertyName("publishStatus")] public string PublishStatus { get; set; }

        [JsonPropertyName("content")] public string Content { get; set; }

        [JsonPropertyName("contentFormat")] public string ContentFormat { get; set; }
        [JsonPropertyName("canonicalUrl")] public string CanonicalUrl { get; set; }
        [JsonPropertyName("license")] public string License { get; set; }
    }
}