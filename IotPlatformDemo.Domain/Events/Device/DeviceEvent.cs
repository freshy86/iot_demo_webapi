using System;
using IotPlatformDemo.Domain.Container;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Events.Device;

public abstract class DeviceEvent(string action, string deviceId) : DeviceContainerObject, IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public string DeviceId { get; } = deviceId;
    public string Action { get; } = action;
    [JsonProperty] private DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}