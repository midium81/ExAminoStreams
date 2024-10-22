using System.Text.Json.Serialization;

namespace ExAminoStreams.API
{
    public class APIError
    {
        [JsonPropertyName("Error")]
        public string Error { get; set; }

    }
}
