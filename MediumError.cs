using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PostMediumGitHubAction
{
    public class MediumError
    {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("code")]
            public int Code { get; set; }
        }
    public class Root
    {
        [JsonPropertyName("errors")]
        public List<MediumError> Errors { get; set; }
    }
}