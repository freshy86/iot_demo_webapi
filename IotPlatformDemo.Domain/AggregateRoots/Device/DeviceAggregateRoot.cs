using IotPlatformDemo.Domain.MaterializedViews;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.AggregateRoots.Device;

public class DeviceAggregateRoot(string deviceId): AggregateRoot(deviceId)
{
    [JsonProperty] public string Name { get; set; }
    [JsonProperty] public string UserId { get; set; }

    public void ApplyTo(DeviceView view)
    {
        view.UserId = UserId;
        view.Name = Name;
    }
}