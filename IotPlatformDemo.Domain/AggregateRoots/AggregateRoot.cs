using System;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.AggregateRoots;

public abstract class AggregateRoot(string partitionKey) : IAggregateRoot
{
    [JsonProperty] public Guid Id { get; } = Guid.NewGuid();
    [JsonProperty] public string PartitionKey { get; } = partitionKey;
}