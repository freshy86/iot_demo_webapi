using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.AggregateRoots.Device;

public class DeviceAggregateRoot(string partitionKey): AggregateRoot(partitionKey)
{
    [JsonProperty] public string DeviceName { get; set; }
    [JsonProperty] public string DeviceId { get; } = partitionKey;
    [JsonProperty] public string UserId { get; set; }
    
}