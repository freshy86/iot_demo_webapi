using System;
using IotPlatformDemo.Domain.Events.Device.V1;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.AggregateRoots.Device;

public class DeviceAggregateRoot(string partitionKey): AggregateRoot(partitionKey)
{
    [JsonProperty] public string DeviceName { get; set; }
    [JsonProperty] public string DeviceId { get; } = partitionKey;
    [JsonProperty] public string UserId { get; set; }

    public void HandleEvent(DeviceCreatedEvent deviceCreatedEvent)
    {
        Console.WriteLine("Handle device created");
        DeviceName = deviceCreatedEvent.DeviceName;
        UserId = deviceCreatedEvent.UserId;
    }
    
    public void HandleEvent(DeviceRenameEvent deviceRenameEvent)
    {
        Console.WriteLine("Handle device rename");
        DeviceName = deviceRenameEvent.NewDeviceName;
    }
}