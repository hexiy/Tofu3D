using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace TofuEngine;

public static class SystemConfig
{
    public static void Configure(string projectPath)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        // if (Environment.CurrentDirectory.EndsWith("Tofu3D", StringComparison.OrdinalIgnoreCase))
        // {
        //     Environment.CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        // }
        // Environment.CurrentDirectory =
        //     Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        if (OperatingSystem.IsMacOS)
        {
            Folders.EditorResources = Path.Combine(AppContext.BaseDirectory, "../Resources/EditorResources");
            if (Directory.Exists(Folders.EditorResources) == false)
            {
                Folders.EditorResources = Path.Combine(AppContext.BaseDirectory, "../../../../EditorResources");
            }
            if (Directory.Exists(Folders.EditorResources) == false)
            {
                Folders.EditorResources = Path.Combine(AppContext.BaseDirectory, "EditorResources");
            }
        }
    
        if (OperatingSystem.IsWindows || OperatingSystem.IsLinux)
        {
            Folders.EditorResources = Path.Combine(AppContext.BaseDirectory, "EditorResources");
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

        Folders.ProjectFullPath = projectPath;
    }
}