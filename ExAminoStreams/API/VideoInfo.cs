using System.Text.Json.Serialization;

namespace ExAminoStreams.API
{
    public class VideoInfo
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
