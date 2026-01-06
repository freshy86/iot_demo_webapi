using System;
using IotPlatformDemo.Domain.AggregateRoots.Device;
using Newtonsoft.Json;
using Action = IotPlatformDemo.Domain.Events.Action;

namespace IotPlatformDemo.Domain.Events.Device.V1;

public class DeviceCreatedEvent(string deviceId, string userId) : DeviceEvent(userId, Action.Create, deviceId)
{
    [JsonProperty] public string DeviceName { get; init; } = "IoT device";

    public override void Apply(DeviceAggregateRoot aggregateRoot)
    {
        Console.WriteLine("Handle device created");
        aggregateRoot.DeviceName = DeviceName;
        aggregateRoot.UserId = UserId;
        aggregateRoot.Id = DeviceId;
    }
}