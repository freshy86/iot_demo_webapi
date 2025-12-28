using System.Collections.Concurrent;
using IotPlatformDemo.Domain.Container;
using IotPlatformDemo.Domain.Events;
using Microsoft.Azure.Cosmos;

namespace IotPlatformDemo.Application.EventStore;

public class CosmosDbEventStore(CosmosClient cosmosClient, string databaseName) : IEventStore
{
    private readonly ConcurrentDictionary<string, Container> _containerRegistry = new();

    private Container GetContainerForObject(IContainerObject containerObject)
    {
        if (_containerRegistry.TryGetValue(containerObject.ContainerName, out var value)) return value;
        value = cosmosClient.GetContainer(
            databaseName, containerObject.ContainerName);
        _containerRegistry[containerObject.ContainerName] = value;
        return value;
    }
    
    public async Task Append(IEvent newEvent)
    {
        var container = GetContainerForObject(newEvent);
        DataObject dataObject = new($"{newEvent}", newEvent, "deviceEvent");
        PartitionKey partitionKey = new(dataObject.PartitionKey);
        await container.CreateItemAsync(dataObject, partitionKey).ConfigureAwait(false);
    }
}