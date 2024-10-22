using System.Text.Json.Serialization;

namespace ExAminoStreams.API
{
    public class StreamData
    {
        [JsonPropertyName("start")]
        public double Start { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("video")]
        public VideoInfo VideoInfo { get; set; }

    }
}
