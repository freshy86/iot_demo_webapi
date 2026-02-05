using IotPlatformDemo.API.Models;
using Microsoft.Azure.Cosmos;

namespace IotPlatformDemo.API.Helpers;

public static class QueryHelpers
{
    private const string SelectionString = "@selection";

    public static QueryDefinition PrepareQuery(string queryString, string selection, Dictionary<string, string> parameters)
    {
        var queryDefinition = new QueryDefinition(queryString.Replace(SelectionString, selection));
        return parameters.Aggregate(queryDefinition, (current, keyValue) => current.WithParameter(keyValue.Key, keyValue.Value));
    }
    
    public static async Task<MultipleItemsResponse<T>> GetMultipleItemsQuery<T>(
        Container container,
        QueryDefinition? totalItemsCountQuery,
        QueryDefinition query, 
        PartitionKey partitionKey, 
        int maxItems = 0, 
        string? continuationToken = null)
    {
        int? totalItems = null;

        if (continuationToken == null)
        {
            using var totalItemsFeed = container.GetItemQueryIterator<Dictionary<string, int>>(totalItemsCountQuery,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = partitionKey
                });

            var totalItemsResponse = await totalItemsFeed.ReadNextAsync();
            Console.WriteLine("Request cost: " + totalItemsResponse.RequestCharge + ", count: " + totalItemsResponse.Count);
            totalItems = totalItemsResponse.FirstOrDefault()?.Values.FirstOrDefault();
        }
        
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
        Console.WriteLine("Request cost: " + response.RequestCharge + ", count: " + response.Count);
        
        return new MultipleItemsResponse<T>
        {
            TotalItems = totalItems,
            Items = response,
            ContinuationToken = nextContinuationToken
        };
    }
}