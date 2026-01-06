using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.MaterializedViews;

public class DeviceView(string id)
{
    public const string IdPrefix = "DeviceView_";
    
    [JsonProperty] public string Name { get; set; }
    [JsonProperty] public string Id { get; init; } = id;
    [JsonProperty] public string UserId { get; set; }
}