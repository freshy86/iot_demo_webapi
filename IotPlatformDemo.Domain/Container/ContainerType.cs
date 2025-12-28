namespace IotPlatformDemo.Domain.Container;

public class ContainerType
{
    public static readonly ContainerType Devices = new("devices");
    
    private readonly string _containerName;

    private ContainerType(string containerName)
    {
        _containerName = containerName;
    }

    public override string ToString()
    {
        return _containerName;
    }
}