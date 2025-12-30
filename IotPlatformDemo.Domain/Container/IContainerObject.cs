using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Container;

public interface IContainerObject
{
    [JsonIgnore] public string PartitionKey { get; }
}