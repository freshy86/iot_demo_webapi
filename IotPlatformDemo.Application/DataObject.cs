using Newtonsoft.Json;

namespace IotPlatformDemo.Application;

public class DataObject<T>(string id, string partitionKey, T entity)
{
    [JsonProperty] public string Id { get; } = id;
    [JsonProperty] public string PartitionKey = partitionKey;
    public T Data { get; } = entity;
    [JsonProperty("_etag")] public string? Etag { get; set; }
    [JsonProperty] public int Ttl => -1;
}