using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PostMediumGitHubAction.Domain
{
    public class MediumError
    {
        [JsonPropertyName("message")] public string Message { get; set; }

        [JsonPropertyName("code")] public int Code { get; set; }
    }
}