using System;
using Newtonsoft.Json;

namespace IotPlatformDemo.Domain.AggregateRoots;

public abstract class AggregrateRoot : IAggregateRoot
{
    [JsonProperty] public Guid Id { get; protected init; }

}