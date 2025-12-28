using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Container;

public interface IContainerObject
{
    [JsonIgnore] public ContainerType TargetContainer { get; }
    [JsonIgnore] public string PartitionKey { get; }
}