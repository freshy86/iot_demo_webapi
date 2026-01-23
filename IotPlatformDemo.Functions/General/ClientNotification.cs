using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IotPlatformDemo.Functions.General;

public record ClientNotification
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NotificationStatus
    {
        Start,
        Success,
        Fail
    }

    [JsonProperty("status")] public required NotificationStatus Status { get; init; }
    [JsonProperty("userId")] public required string UserId { get; init; }
    [JsonProperty("id")] public required string OrchestrationId { get; init; }
    [JsonProperty("result")] public object? Result { get; init; }
}