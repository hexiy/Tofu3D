using System.Reflection;
using System.Runtime.Loader;

public class ScriptLoadContext : AssemblyLoadContext
{
    public ScriptLoadContext() : base(isCollectible: true) {}

    protected override Assembly Load(AssemblyName assemblyName)
    {
        // Add custom logic for resolving dependencies if needed
        return null;
    }
}