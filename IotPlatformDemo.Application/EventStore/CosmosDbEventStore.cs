using IotPlatformDemo.Domain.Events;
using Microsoft.Azure.Cosmos;

namespace IotPlatformDemo.Application.EventStore;

public class CosmosDbEventStore(Container targetContainer) : IEventStore
{
    public async Task Append(Event eventToAdd)
    {
        DataObject<Event> dataObject = new($"{eventToAdd.Id}", eventToAdd.PartitionKey, eventToAdd);
        PartitionKey partitionKey = new(dataObject.PartitionKey);
        await targetContainer.CreateItemAsync(dataObject, partitionKey).ConfigureAwait(false);
    }
}