using IotPlatformDemo.Domain.Events;

namespace IotPlatformDemo.Application.EventStore;

public interface IEventStore
{
    Task Append(Event newObject);
}