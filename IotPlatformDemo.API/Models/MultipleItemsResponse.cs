using Newtonsoft.Json;

namespace IotPlatformDemo.API.Models;

public record MultipleItemsResponse<T>
{
    public required IEnumerable<T> Items { get; init; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public required int? TotalItems { get; init; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? ContinuationToken { get; init; }
}