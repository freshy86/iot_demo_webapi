using System;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.AggregateRoots;

public abstract class AggregateRoot(string partitionKey) : IAggregateRoot
{
    [JsonProperty] public string Id { get; set; } = Guid.NewGuid().ToString("N");
    [JsonProperty] public string PartitionKey { get; init; } = partitionKey;
    [JsonProperty("_etag")] public string ETag { get; init; }
}