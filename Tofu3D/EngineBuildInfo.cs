namespace Tofu3D;

public static class EngineBuildInfo
{
    private static string? _versionCached = null;

    public static string Version
    {
        get
        {
            return _versionCached ??= System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}