using System.IO;
using System.Linq;

namespace TofuEngine;

public class Folders
{
    public static string ProjectFullPath;
    public static string EditorResources;
    public static string EditorResourcesProjectFiles => TofuPath.Combine(EditorResources, "ProjectFiles");
    public static string EditorResourcesFonts => TofuPath.Combine(EditorResources, "Fonts");
    public static string EditorResourcesTextures => TofuPath.Combine(EditorResources, "Textures");

    public static string ProjectSettings => TofuPath.Combine(ProjectFullPath, "ProjectSettings");
    public static string Library => TofuPath.Combine(ProjectFullPath, "Library");
    public static string Data => TofuPath.Combine(ProjectFullPath, "Data");
    public static string ModelsInLibrary => TofuPath.Combine(Library, "Models");
    public static string TexturesInLibrary => TofuPath.Combine(Library, "Textures");
    public static string TextureAtlasesInLibrary => TofuPath.Combine(Library, "TextureAtlases");
    public static string MaterialsInLibrary => TofuPath.Combine(Library, "Materials");
    public static string MeshesInLibrary => TofuPath.Combine(Library, "Meshes");
    public static string ThumbnailsInLibrary => TofuPath.Combine(Library, "Thumbnails");
    public static string SceneThumbnailsInLibrary => TofuPath.Combine(ThumbnailsInLibrary, "Scenes");
    public static string TempInLibrary => TofuPath.Combine(Library, "Temp");

    public static string Assets => TofuPath.Combine(ProjectFullPath, "Assets");
    public static string Dlls => TofuPath.Combine(ProjectFullPath, "DLLs");
    public static string Scripts => TofuPath.Combine(Assets, "Scripts");

    public static string TexturesInAssets => TofuPath.Combine(Assets, "2D");

    public static string ShadersInAssets => TofuPath.Combine(Assets, "Shaders");
    public static string ScenesInAssets => TofuPath.Combine(Assets, "Scenes");

    public static string MaterialsInAssets => TofuPath.Combine(Assets, "Materials");

    public static string ModelsInAssets => TofuPath.Combine(Assets, "3D");
    public static string BasicModelsInAssets => TofuPath.Combine(Assets, "3D", "Basic");

    public static void ValidateProjectFolder(string projectPath)
    {
        ProjectFullPath = projectPath;
        if (Directory.Exists(ProjectFullPath) == false)
        {
            Directory.CreateDirectory(ProjectFullPath);
        }

        var foldersInside = Directory.GetDirectories(ProjectFullPath);
        if (foldersInside.Length < 2)
        {
            foreach (string dirPath in Directory.GetDirectories(EditorResourcesProjectFiles, "*",
                         SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(EditorResourcesProjectFiles, ProjectFullPath));
            }

            foreach (string newPath in Directory.GetFiles(EditorResourcesProjectFiles, "*.*",
                         SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(EditorResourcesProjectFiles, ProjectFullPath), true);
            }
        }
        
        Directory.CreateDirectory(Path.Combine(ProjectFullPath, "ProjectSettings"));
    }

    public static void CreateDefaultFolders()
    {
        Directory.CreateDirectory(Library);
        Directory.CreateDirectory(Data);
        Directory.CreateDirectory(ModelsInLibrary);
        Directory.CreateDirectory(TexturesInLibrary);
        Directory.CreateDirectory(TextureAtlasesInLibrary);
        Directory.CreateDirectory(MaterialsInLibrary);
        Directory.CreateDirectory(MeshesInLibrary);
        Directory.CreateDirectory(TempInLibrary);
        Directory.CreateDirectory(ThumbnailsInLibrary);
        Directory.CreateDirectory(SceneThumbnailsInLibrary);
        Directory.CreateDirectory(Scripts);
        Directory.CreateDirectory(Dlls);
    }

    /// <summary>
    ///     From "Desktop/project/bin/Assets/2D/xx.png" to "Assets/2D/xx.png"
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetPathRelativeToAssetsFolder(string path)
    {
        if (path.Length == 0)
        {
            return Assets;
        }

        // return TofuPath.Combine(Assets, Path.GetRelativePath(Assets, path));
        return Path.GetRelativePath(Assets, path);
    }

    public static string GetParentFolder(string path)
    {
        if (path.Length == 0)
        {
            return path;
        }

        int lastIndexOfDirectorySeparator = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);

        if (lastIndexOfDirectorySeparator == -1)
        {
            return path;
        }

        return path.Remove(lastIndexOfDirectorySeparator);
    }


    public static string Get2DAssetPath(string assetName) => TofuPath.Combine(TexturesInAssets, assetName);

    public static string GetEditorResourcePath(string assetName) => TofuPath.Combine(EditorResources, assetName);

    /// <summary>
    ///     From "Desktop/project/bin/Assets/2D/xx.png" to "bin/Assets/2D/xx.png"
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetPathRelativeToProjectFolder(string path)
    {
        if (path.Length == 0)
        {
            return Assets;
        }

        return Path.GetRelativePath(ProjectFullPath, path);
    }
}