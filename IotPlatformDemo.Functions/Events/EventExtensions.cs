using IotPlatformDemo.Domain.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IotPlatformDemo.Functions.Events;

public static class EventExtensions
{
    extension(string eventString)
    {
        public T DeserializeEvent<T>() where T: Event
        {
            var eventsAssembly = typeof(Event).Assembly;
            var jsonObject = (JsonConvert.DeserializeObject(eventString) as JObject)!;
            var eventType = eventsAssembly.GetType($"{jsonObject.GetValue("version")}", true)!;
            return (jsonObject.ToObject(eventType) as T)!;
        }
    }
}