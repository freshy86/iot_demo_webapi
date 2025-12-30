using System;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Events.Device;

public class DeviceEvent(Action action, string deviceId) : Event(EventType.DeviceEvent, action, deviceId)
{
    public string DeviceId { get; } = deviceId;
    [JsonProperty] private DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}