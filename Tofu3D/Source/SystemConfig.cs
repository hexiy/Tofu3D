using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace Tofu3D;

public static class SystemConfig
{
    public static void Configure()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        if (Environment.CurrentDirectory.EndsWith("Tofu3D", StringComparison.OrdinalIgnoreCase))
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        Folders.EngineBinPath = Environment.CurrentDirectory;

        DirectoryInfo directoryInfo = Directory.GetParent(Environment.CurrentDirectory);



        string projectFullPath = Directory.GetDirectories(directoryInfo.FullName, searchPattern: "tofuProject",
            searchOption: SearchOption.AllDirectories).FirstOrDefault() ?? "";
        while (projectFullPath == "")
        {
            directoryInfo = directoryInfo.Parent;
            projectFullPath = Directory.GetDirectories(directoryInfo.FullName, searchPattern: "tofuProject",
                searchOption: SearchOption.AllDirectories).FirstOrDefault() ?? "";
        }

        Folders.ProjectFullPath = projectFullPath;

        Environment.CurrentDirectory = Folders.ProjectFullPath;
    }
}