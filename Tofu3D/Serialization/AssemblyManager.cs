using System.IO;
using System.Reflection;

namespace Tofu3D;

public static class AssemblyManager
{
    public static void CompileScriptsAssembly(string path)
    {
        var currentAssemblyPath = Assembly.GetExecutingAssembly().Location;

        using (FileStream fs = new(currentAssemblyPath, FileMode.Open))
        {
            using (FileStream newAssemblyFileStream = new(path, FileMode.Create))
            {
                fs.CopyTo(newAssemblyFileStream);
            }
        }
    }

    public static Assembly LoadScriptsAssembly(string path) => Assembly.LoadFile(path);
}