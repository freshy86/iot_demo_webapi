using IotPlatformDemo.Domain.Events;
using Microsoft.Azure.Cosmos;

namespace IotPlatformDemo.Application.EventStore;

public class CosmosDbEventStore(Container targetContainer) : IEventStore
{
    public async Task Append(Event eventToAdd)
    {
        PartitionKey partitionKey = new(eventToAdd.PartitionKey);
        await targetContainer.CreateItemAsync(eventToAdd, partitionKey).ConfigureAwait(false);
    }
}