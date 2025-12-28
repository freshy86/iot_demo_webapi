using System;
using IotPlatformDemo.Domain.Container;
using IotPlatformDemo.Domain.Events;
using Action = IotPlatformDemo.Domain.Events.Action;

namespace IotPlatformDemo.Domain;

public class Event(string name, Action action, ContainerType targetContainer, string partitionKey) : IEvent
{
    public ContainerType TargetContainer { get; } = targetContainer;
    public string PartitionKey { get; } = partitionKey;
    public Guid Id { get; } = Guid.NewGuid();
    public Action Action { get; } = action;
    public string Name { get; } = name;
}