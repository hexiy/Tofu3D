﻿using System.IO;

namespace Tofu3D;

public class Folders
{
    public static string EngineFolderPath => Environment.CurrentDirectory;
    public static string Resources => "Resources";
    public static string Library => "Library";
    public static string ModelsInLibrary => Path.Combine(Library, "Models");
    public static string TexturesInLibrary => Path.Combine(Library, "Textures");
    public static string MaterialsInLibrary => Path.Combine(Library, "Materials");
    public static string MeshesInLibrary => Path.Combine(Library, "Meshes");

    public static string Assets => Path.Combine(Environment.CurrentDirectory, "Assets");
    public static string TexturesInAssets => Path.Combine(Assets, "2D");

    public static string ShadersInAssets => Path.Combine(Assets, "Shaders");

    public static string MaterialsInAssets => Path.Combine(Assets, "Materials");

    public static string ModelsInAssets => Path.Combine(Assets, "3D");

    public static void CreateDefaultFolders()
    {
        Directory.CreateDirectory(Library);
        Directory.CreateDirectory(ModelsInLibrary);
        Directory.CreateDirectory(TexturesInLibrary);
        Directory.CreateDirectory(MaterialsInLibrary);
        Directory.CreateDirectory(MeshesInLibrary);
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

        return Path.Combine("Assets", Path.GetRelativePath(Assets, path));
    }

    public static string Get2DAssetPath(string assetName) => Path.Combine(TexturesInAssets, assetName);

    public static string GetResourcePath(string assetName) => Path.Combine(Resources, assetName);

    /// <summary>
    ///     From "Desktop/project/bin/Assets/2D/xx.png" to "bin/Assets/2D/xx.png"
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetPathRelativeToEngineFolder(string path)
    {
        if (path.Length == 0)
        {
            return Assets;
        }

        return Path.GetRelativePath(EngineFolderPath, path);
    }
}