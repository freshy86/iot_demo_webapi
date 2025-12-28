using System.Collections.Concurrent;
using IotPlatformDemo.Domain;
using IotPlatformDemo.Domain.Container;
using Microsoft.Azure.Cosmos;

namespace IotPlatformDemo.Application.EventStore;

public class CosmosDbEventStore(CosmosClient cosmosClient, string databaseName) : IEventStore
{
    private readonly ConcurrentDictionary<ContainerType, Container> _containerRegistry = new();

    private Container GetContainerForEvent(Event eventToAdd)
    {
        if (_containerRegistry.TryGetValue(eventToAdd.TargetContainer, out var value)) return value;
        value = cosmosClient.GetContainer(
            databaseName, eventToAdd.TargetContainer.ToString());
        _containerRegistry[eventToAdd.TargetContainer] = value;
        return value;
    }
    
    public async Task Append(Event eventToAdd)
    {
        var container = GetContainerForEvent(eventToAdd);
        DataObject<Event> dataObject = new($"{eventToAdd.Id}", eventToAdd.PartitionKey, eventToAdd);
        PartitionKey partitionKey = new(dataObject.PartitionKey);
        await container.CreateItemAsync(dataObject, partitionKey).ConfigureAwait(false);
    }
}