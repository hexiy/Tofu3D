using System.IO;

namespace TofuEngine;

public static class AssetPathConverter
{
    public static string ToProjectRelativePath(string path)
    {
        if (path.Length == 0)
        {
            return path;
        }

        return Path.GetRelativePath(Environment.CurrentDirectory, path);
    }
}