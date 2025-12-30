namespace IotPlatformDemo.Domain.Container;

public class ContainerType
{
    public static readonly ContainerType Data = new("data");
    
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