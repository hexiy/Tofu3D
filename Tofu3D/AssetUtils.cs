using System.IO;

namespace Tofu3D;

public class AssetUtils
{
    public static bool IsShader(string path)
    {
        return path.Contains(".glsl");
    }

    public static bool Exists(string path)
    {
        return File.Exists(path);
    }

    public static string ValidateAssetPath(ref string assetPath)
    {
        assetPath = ValidateAssetPath(assetPath);
        return assetPath;
    }

    public static string ValidateAssetPath(string assetPath)
    {
        assetPath = assetPath.Replace("\\", "/");
        // assetPath = assetPath.Replace("net8", "net7");
        assetPath = assetPath.Replace("net7", "net8");
        // assetPath = assetPath.Replace(" ", "\\ ");
        bool isValid = File.Exists(assetPath);
        if (isValid) return assetPath;
        bool existsInAssetFolder = File.Exists(Path.Combine(Folders.Assets, assetPath));
        if (existsInAssetFolder)
        {
            assetPath = Path.Combine(Folders.Assets, assetPath);
            assetPath = Folders.GetPathRelativeToAssetsFolder(assetPath);
        }
        else
        {
            assetPath = Folders.GetPathRelativeToEngineFolder(assetPath);
        }

        if (Exists(assetPath) == false)
        {
            string assetPathInAssetsFolder = Path.Combine("Assets", assetPath);
            if (File.Exists(assetPathInAssetsFolder)) assetPath = assetPathInAssetsFolder;
        }

        // if (AssetUtils.Exists(assetPath) == false)
        // {
        // 	string message = $"Couldn't find asset:{assetPath}";
        // 	Debug.LogError(message);
        // 	// throw new FileNotFoundException(message);
        // }

        return assetPath;
    }
}