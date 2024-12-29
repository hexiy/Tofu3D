using System.Globalization;
using System.IO;
using System.Threading;

namespace Tofu3D;

public static class SystemConfig
{
    public static void Configure()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        Folders.EngineFullPath = Directory.GetParent(Environment.CurrentDirectory).FullName;

        Folders.ProjectFullPath = Path.Combine(
            Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName,
            "tofuProject");

        Environment.CurrentDirectory = Folders.ProjectFullPath;
    }
}