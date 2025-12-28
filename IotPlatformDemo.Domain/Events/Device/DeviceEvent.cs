using System;
using IotPlatformDemo.Domain.Container;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Events.Device;

public class DeviceEvent(string action, string deviceId) : Event(ContainerType.Devices, deviceId, action, nameof(DeviceEvent))
{
    public string DeviceId { get; } = deviceId;
    [JsonProperty] private DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}