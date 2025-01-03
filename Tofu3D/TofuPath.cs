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
        return Combine(PathScope.None, paths);
    }

    public static string Combine(PathScope pathScope, params string[] paths)
    {
        string path = Microsoft.IO.Path.Combine(paths);
        if (pathScope == PathScope.Project)
        {
            path = Microsoft.IO.Path.GetRelativePath(Folders.ProjectFullPath, path);
        }
        else if (pathScope == PathScope.Library)
        {
            path = Microsoft.IO.Path.GetRelativePath(Folders.Library, path);
        }
        else if (pathScope == PathScope.Assets)
        {
            path = Microsoft.IO.Path.GetRelativePath(Folders.Assets, path);
        }

        return path;
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        return Microsoft.IO.Path.GetFileNameWithoutExtension(path);
    }
}