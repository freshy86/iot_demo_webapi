namespace IotPlatformDemo.Domain.Events.Device;

public class DeviceCreatedEvent(string deviceId, string userId) : DeviceEvent(userId, Action.Create, deviceId)
{
}