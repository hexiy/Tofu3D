using System.IO;

namespace Tofu3D;

public class Folders
{
    public static string EngineBinPath;
    public static string ProjectFullPath;
    public static string Resources => TofuPath.Combine(ProjectFullPath, "Resources");
    public static string FontsInResources => TofuPath.Combine(Resources, "Fonts");
    public static string Library => TofuPath.Combine(ProjectFullPath, "Library");
    public static string Data => TofuPath.Combine(ProjectFullPath, "Data");
    public static string ModelsInLibrary => TofuPath.Combine(Library, "Models");
    public static string TexturesInLibrary => TofuPath.Combine(Library, "Textures");
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

    public static void CreateDefaultFolders()
    {
        Directory.CreateDirectory(Library);
        Directory.CreateDirectory(Data);
        Directory.CreateDirectory(ModelsInLibrary);
        Directory.CreateDirectory(TexturesInLibrary);
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

    public static string GetResourcePath(string assetName) => TofuPath.Combine(Resources, assetName);

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