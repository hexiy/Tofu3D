namespace Tofu3D;

public static class ProjectDataManager
{
    public static string? EditorVersion
    {
        get { return Serializer.ReadFileJSON<string>(TofuPath.Combine(Folders.ProjectSettings, "ProjectVersion.txt")); }
        set { Serializer.SaveFileJSON<string>(TofuPath.Combine(Folders.ProjectSettings, "ProjectVersion.txt"), value); }
    }
}