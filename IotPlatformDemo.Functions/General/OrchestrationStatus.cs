using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IotPlatformDemo.Functions.General;

public record OrchestrationStatus
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatusCode
    {
        Start,
        Finish,
        Fail
    }

    [JsonProperty] public required StatusCode Status { get; init; }
    [JsonProperty] public required string UserId { get; init; }
    [JsonProperty] public required string OrchestrationId { get; init; }
}