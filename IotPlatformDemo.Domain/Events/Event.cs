using System;

namespace IotPlatformDemo.Domain.Events;

public class Event(EventType type, Action action, string partitionKey) : IEvent
{ 
    public string PartitionKey { get; } = partitionKey;
    public Guid Id { get; } = Guid.NewGuid();
    public Action Action { get; } = action;
    public EventType Type { get; } = type;
}