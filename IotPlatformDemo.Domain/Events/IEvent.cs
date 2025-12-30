using System;
using IotPlatformDemo.Domain.Container;

namespace IotPlatformDemo.Domain.Events;

public interface IEvent : IContainerObject
{
    public Guid Id { get; }
    public Action Action { get; }
    public EventType Type { get; }
}