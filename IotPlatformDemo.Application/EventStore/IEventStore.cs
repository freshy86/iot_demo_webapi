using IotPlatformDemo.Domain.Container;
using IotPlatformDemo.Domain.Events;

namespace IotPlatformDemo.Application.EventStore;

public interface IEventStore
{
    Task Append(IEvent newObject);
}