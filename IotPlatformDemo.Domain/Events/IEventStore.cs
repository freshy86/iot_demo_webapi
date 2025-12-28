namespace IotPlatformDemo.Domain.Events;

public interface IEventStore
{
    void Append<T>(T newEvent) where T : IEvent;
}