using System;
using IotPlatformDemo.Domain.Container;
using IotPlatformDemo.Domain.Events;

namespace IotPlatformDemo.Domain;

public class Event(ContainerType targetContainer, string partitionKey, string action, string name) : IEvent
{
    public ContainerType TargetContainer { get; } = targetContainer;
    public string PartitionKey { get; } = partitionKey;
    public Guid Id { get; } = Guid.NewGuid();
    public string Action { get; } = action;
    public string Name { get; } = name;
}