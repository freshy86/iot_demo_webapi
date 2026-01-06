using IotPlatformDemo.Domain.AggregateRoots.Device;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Events.Device.V1;

public abstract class DeviceEvent(string userId, Action action, string deviceId) : Event(userId, EventType.DeviceEvent, action, 
    partitionKey: deviceId)
{
    [JsonProperty] public string DeviceId { get; init; } = deviceId;

    public abstract void Apply(DeviceAggregateRoot aggregateRoot);
}