using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace TofuEngine;

public static class SystemConfig
{
    public static void Configure()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        // if (Environment.CurrentDirectory.EndsWith("Tofu3D", StringComparison.OrdinalIgnoreCase))
        // {
        //     Environment.CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        // }
        // Environment.CurrentDirectory =
        //     Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        // for macos only
        Folders.EditorResources = Path.Combine(AppContext.BaseDirectory, "../Resources/EditorResources");
        if (Directory.Exists(Folders.EditorResources) == false)
        {
            Folders.EditorResources = Path.Combine(AppContext.BaseDirectory, "../../../../EditorResources");
        }

        Debug.Log($"Folders.EditorResources:{Folders.EditorResources}");

        // DirectoryInfo directoryInfo = Directory.GetParent(Environment.CurrentDirectory);


        // string projectFullPath = Directory.GetDirectories(directoryInfo.FullName, searchPattern: "tofuProject",
        //     searchOption: SearchOption.AllDirectories).FirstOrDefault() ?? "";
        // while (projectFullPath == "")
        // {
        //     directoryInfo = directoryInfo.Parent;
        //     projectFullPath = Directory.GetDirectories(directoryInfo.FullName, searchPattern: "tofuProject",
        //         searchOption: SearchOption.AllDirectories).FirstOrDefault() ?? "";
        // }
        string projectFullPath = "/Users/hexiy/dev/Game Engine dev/Tofu3D/tofuProject";

        Folders.ProjectFullPath = projectFullPath;
        Environment.CurrentDirectory = Folders.ProjectFullPath;
    }
}