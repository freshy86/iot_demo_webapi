using System;
using IotPlatformDemo.Domain.Container;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Events.Device;

public abstract class DeviceEvent(string action, string deviceId) : DeviceContainerObject(deviceId), IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public string DeviceId { get; } = deviceId;
    public string Action { get; } = action;
    public string Name => nameof(DeviceEvent);
    [JsonProperty] private DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}