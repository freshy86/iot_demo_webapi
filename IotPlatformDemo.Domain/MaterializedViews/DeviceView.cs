using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IotPlatformDemo.Domain.MaterializedViews;

[JsonConverter(typeof(StringEnumConverter))]
public enum ViewType
{
    DeviceView
}

public class DeviceView(string id, string partitionKey)
{
    [JsonProperty] public string Name { get; set; }
    [JsonProperty] public ViewType Type { get; init; } = ViewType.DeviceView;
    [JsonProperty] public string Id { get; init; } = id;
    [JsonProperty] public string UserId { get; set; }
    [JsonProperty] public string PartitionKey { get; init; } = partitionKey;
}