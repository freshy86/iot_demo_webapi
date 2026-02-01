using System;
using IotPlatformDemo.Domain.Helpers;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.Events;

public class Event(string userId, EventType type, Action action, string partitionKey) : IEvent
{ 
    [JsonProperty] public string PartitionKey { get; init; } = partitionKey;
    [JsonProperty] public string UserId { get; init; } = userId;
    [JsonProperty] public string Id { get; init; } = GuidHelpers.NewSimpleGuidString();
    [JsonProperty] public Action Action { get; init; } = action;
    [JsonProperty] public EventType Type { get; init; } = type;
    [JsonProperty] public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    [JsonProperty] public string Version => $"{GetType().FullName}";
}