namespace IotPlatformDemo.Functions;

public static class ChangeFeedContainerPrefixConstants
{
    //Only way I found to have different prefixes for debug and release builds, is there a better way?
#if DEBUG
    public const string Extension = "-debug";
#else
    public const string Extension = "";
#endif
}