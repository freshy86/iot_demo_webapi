using System;
using IotPlatformDemo.Domain.AggregateRoots.Device;
using Newtonsoft.Json;
using Action = IotPlatformDemo.Domain.Events.Base.V1.Action;

namespace IotPlatformDemo.Domain.Events.Device.V1;

public class DeviceRenameEvent(string deviceId, string userId, string newDeviceName): DeviceEvent(userId, Action.Update, deviceId)
{
    [JsonProperty] public string NewDeviceName { get; } = newDeviceName;

    public override void Apply(DeviceAggregateRoot aggregateRoot)
    {
        Console.WriteLine("Handle device rename");
        aggregateRoot.DeviceName = NewDeviceName;
    }
}