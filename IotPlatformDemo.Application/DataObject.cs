using IotPlatformDemo.Domain.Container;
using Newtonsoft.Json;

namespace IotPlatformDemo.Application;

public class DataObject(string id, IContainerObject containerObject, string type)
{
    [JsonProperty] public string Id { get; } = id;
    [JsonProperty] public string PartitionKey => containerObject.PartitionKey;
    [JsonProperty] public string Type { get; } = type;
    [JsonProperty] public object Data { get; } = containerObject;
    [JsonProperty("_etag")] public string? Etag { get; set; }
    [JsonProperty] public int Ttl => -1;
}