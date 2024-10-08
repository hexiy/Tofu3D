using System.IO;

namespace Tofu3D;

public class AssetUtils
{
    private static Dictionary<string, bool> ExistingAssets = new();

    public static bool IsShader(string path) => path.Contains(".glsl");

    public static bool Exists(string path) => File.Exists(path);

    // ExistingAssets.TryGetValue(path, out var existing);
    //
    // if (existing)
    // {
    //     return true;
    // }
    // else
    // {
    //     ExistingAssets[path] = File.Exists(path);
    //     return ExistingAssets[path];
    // }
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

        // bool isValid = Exists(assetPath);
        // if (isValid) return assetPath;
        var existsInAssetFolder = Exists(Path.Combine(Folders.Assets, assetPath));
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
            var assetPathInAssetsFolder = Path.Combine("Assets", assetPath);
            if (Exists(assetPathInAssetsFolder))
            {
                assetPath = assetPathInAssetsFolder;
            }
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