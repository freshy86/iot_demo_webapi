using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IotPlatformDemo.Domain.Events;

[JsonConverter(typeof(StringEnumConverter))]
public enum EventType
{
    DeviceEvent    
}