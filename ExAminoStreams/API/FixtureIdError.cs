using System.Text.Json.Serialization;

namespace ExAminoStreams.API
{
    public class FixtureIdError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
