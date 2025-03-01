namespace Tofu3D;

public static class EngineBuildInfo
{
    private static string? _versionCached = null;

    public static string Version
    {
        get
        {
            if (_versionCached == null)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName();

                _versionCached =
                    $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
            }

            return _versionCached;
        }
    }
}