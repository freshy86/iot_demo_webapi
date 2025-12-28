using System;

namespace IotPlatformDemo.Domain.Events.Device;

public class DeviceCreatedEvent(string deviceId, string userId, string name) : DeviceEvent("CREATE", deviceId)
{
    public string Name { get; } = name;
    public string UserId { get; } = userId;
}