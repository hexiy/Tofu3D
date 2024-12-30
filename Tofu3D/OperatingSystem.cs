namespace Tofu3D;

public static class OperatingSystem
{
#if OS_MACOS
    public const bool IsMacOS = true;
    public const bool IsWindows = false;
    public const bool IsLinux = false;
#elif OS_WINDOWS
    public const bool IsMacOS = false;
    public const bool IsWindows = true;
    public const bool IsLinux = false;
#elif OS_LINUX
    public const bool IsMacOS = false;
    public const bool IsWindows = false;
    public const bool IsLinux = true;
#endif
}