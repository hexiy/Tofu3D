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

        // if (Path.IsPathRooted(path))
        // {
        //     return path;
        // }

        // return Path.Combine(Folders.ProjectFullPath, path);

        return Path.GetRelativePath(Folders.ProjectFullPath, path);
    }
}