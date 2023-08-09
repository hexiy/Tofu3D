namespace Tofu3D;

public static class OperatingSystem
{
#if OS_MACOS
    public static bool IsMacOS => true;
    public static bool IsWindows => false;
#else
    public static bool IsMacOS => false;
    public static bool IsWindows => true;
#endif
}