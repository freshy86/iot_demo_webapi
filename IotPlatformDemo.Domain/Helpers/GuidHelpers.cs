using System;

namespace IotPlatformDemo.Domain.Helpers;

public static class GuidHelpers
{
    public static string NewSimpleGuidString()
    {
        return Guid.NewGuid().ToString("N");
    }
}