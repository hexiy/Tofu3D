namespace Tofu3D;

public static class TofuPath
{
    public enum PathScope
    {
        None,
        Project,
        Library,
        Assets
    }

    public static string Combine(params string[] paths)
    {
        return System.IO.Path.Combine(paths);
    }

    public static string CombineRelativeTo(PathScope pathScope, params string[] paths)
    {
        if (pathScope == PathScope.None)
        {
            return Combine(paths);
        }

        string relativeTo = "";
        if (pathScope == PathScope.Project)
        {
            relativeTo = Folders.ProjectFullPath;
        }
        else if (pathScope == PathScope.Library)
        {
            relativeTo = Folders.Library;
        }
        else if (pathScope == PathScope.Assets)
        {
            relativeTo = Folders.Assets;
        }

        return CombineRelativeTo(relativeTo, paths);
    }

    public static string CombineRelativeTo(string? relativeTo, params string[] paths)
    {
        string path = System.IO.Path.Combine(paths);
        if (relativeTo != null)
        {
            path = System.IO.Path.GetRelativePath(relativeTo, path);
        }

        return path;
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }
}