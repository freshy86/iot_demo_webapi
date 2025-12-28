using System;
using IotPlatformDemo.Domain.Container;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Events.Device;

public class DeviceEvent(Action action, string deviceId) : Event(nameof(DeviceEvent), action, ContainerType.Devices, deviceId)
{
    public string DeviceId { get; } = deviceId;
    [JsonProperty] private DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}