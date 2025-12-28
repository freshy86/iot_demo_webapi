namespace IotPlatformDemo.API.Models;

public record CreateDeviceDto
(
    string Name,
    DeviceType Type
);