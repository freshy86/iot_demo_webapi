using System;

namespace IotPlatformDemo.Domain.Events.Base.V1;

public interface IEvent
{
    public string PartitionKey { get; }
    public string Id { get; }
    public Action Action { get; }
    public EventType Type { get; }
    public DateTimeOffset CreatedAt { get; }
    public string Version { get; }
}