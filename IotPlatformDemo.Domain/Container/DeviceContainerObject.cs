using System.Text.Json.Serialization;

namespace IotPlatformDemo.Domain.Container;

public abstract class DeviceContainerObject(string partitionKey) : IContainerObject
{
    [JsonIgnore] public string ContainerName => "devices";
    public string PartitionKey { get; } = partitionKey;
}