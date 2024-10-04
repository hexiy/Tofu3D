using System.IO;

namespace Tofu3D;

public class Folders
{
    public static string EngineFolderPath => Environment.CurrentDirectory;
    public static string Resources => "Resources";

    public static string Assets => Path.Combine(Environment.CurrentDirectory, "Assets");
    public static string Textures => Path.Combine(Assets, "2D");

    public static string Shaders => Path.Combine(Assets, "Shaders");

    public static string Materials => Path.Combine(Assets, "Materials");

    public static string Models => Path.Combine(Assets, "3D");

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

    public static string Get2DAssetPath(string assetName) => Path.Combine(Textures, assetName);

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