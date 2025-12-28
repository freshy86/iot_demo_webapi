using System;

namespace IotPlatformDemo.Domain.Events.Device;

public class DeviceCreatedEvent(string deviceId, string userId) : DeviceEvent("CREATE", deviceId)
{
    public string UserId { get; } = userId;
}