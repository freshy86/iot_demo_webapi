namespace IotPlatformDemo.Domain.Container;

public abstract class DeviceContainerObject : IContainerObject
{
    public string ContainerName => "Devices";
    public string PartitionKey => "/deviceId";
}