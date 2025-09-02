using System.Reflection;

namespace TofuEngine;

public static class EngineBuildInfo
{
    private static string? _versionCached = null;

    public static string Version
    {
        get
        {
            if (_versionCached == null)
            {
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                AssemblyName assemblyName = assembly.GetName();

                _versionCached =
                    $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
            }

            return _versionCached;
        }
    }
}