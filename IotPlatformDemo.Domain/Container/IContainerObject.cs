namespace IotPlatformDemo.Domain.Container;

public interface IContainerObject
{
    public string ContainerName { get; }
    
    public string PartitionKey { get; }
}