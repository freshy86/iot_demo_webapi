using IotPlatformDemo.API.Models;
using Microsoft.Azure.Cosmos;

namespace IotPlatformDemo.API.Helpers;

public static class QueryHelpers
{
    public static async Task<MultipleItemsResponse<T>> GetMultipleItemsQuery<T>(Container container,
        QueryDefinition query, PartitionKey partitionKey, int maxItems = 0, string? continuationToken = null)
    {
        using var feed = container.GetItemQueryIterator<T>(
            query,
            string.IsNullOrWhiteSpace(continuationToken) ? null : continuationToken,
            requestOptions: new QueryRequestOptions
            {
                MaxItemCount = maxItems,
                PartitionKey = partitionKey
            }
        );
            
        var response = await feed.ReadNextAsync();
        var nextContinuationToken = feed.HasMoreResults ? response.ContinuationToken : null;
            
        return new MultipleItemsResponse<T>
        {
            Items = response,
            ContinuationToken = nextContinuationToken
        };
    }
}