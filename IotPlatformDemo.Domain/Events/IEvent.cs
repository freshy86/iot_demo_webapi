using System;
using IotPlatformDemo.Domain.Container;

namespace IotPlatformDemo.Domain.Events;

public interface IEvent : IContainerObject
{
    public Guid Id { get; }
    public string Action { get; }
    
}