using System;
using IotPlatformDemo.Domain.Helpers;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.AggregateRoots;

public abstract class AggregateRoot(string partitionKey) : IAggregateRoot
{
    [JsonProperty] public string Id { get; set; } = GuidHelpers.NewSimpleGuidString();
    [JsonProperty] public string PartitionKey { get; init; } = partitionKey;
    [JsonProperty("_etag")] public string ETag { get; init; }
}