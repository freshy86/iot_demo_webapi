using System;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Events.Base.V1;

public class Event(string userId, EventType type, Action action, string partitionKey) : IEvent
{ 
    [JsonProperty] public string PartitionKey { get; } = partitionKey;
    [JsonProperty] public string UserId { get; } = userId;
    [JsonProperty] public string Id { get; } = Guid.NewGuid().ToString("N");
    [JsonProperty] public Action Action { get; } = action;
    [JsonProperty] public EventType Type { get; } = type;
    [JsonProperty] public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    [JsonProperty] public string Version => $"{GetType().FullName}";
}