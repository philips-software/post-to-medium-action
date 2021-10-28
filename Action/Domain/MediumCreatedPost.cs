using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PostMediumGitHubAction.Domain
{
    public class MediumCreatedPost
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("authorId")]
        public string AuthorId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("canonicalUrl")]
        public string CanonicalUrl { get; set; }

        [JsonPropertyName("publishStatus")]
        public string PublishStatus { get; set; }

        [JsonPropertyName("license")]
        public string License { get; set; }

        [JsonPropertyName("licenseUrl")]
        public string LicenseUrl { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("publicationId")]
        public string PublicationId { get; set; }
    }
}
