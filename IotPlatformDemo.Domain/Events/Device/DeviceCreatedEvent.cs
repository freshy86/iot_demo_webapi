namespace IotPlatformDemo.Domain.Events.Device;

public class DeviceCreatedEvent(string deviceId, string userId) : DeviceEvent(Action.Create, deviceId)
{
    public string UserId { get; } = userId;
}