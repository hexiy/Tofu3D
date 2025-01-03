using System.IO;
using System.Text;

public static class AssetPathExtensions
{
    private static readonly string LibraryString1 = Path.DirectorySeparatorChar + "Library";
    private static readonly string LibraryString2 = "Library" + Path.DirectorySeparatorChar;

    public static string GetPathOfImportParametersOfSourceAssetFile(string sourceFilePath)
    {
        return sourceFilePath + ".importparameters";
    }

    // from /Assets/car.obj to /Library/car.asset
    public static string GetPathOfAssetInLibraryFromSourceAssetPathOrName(string fileName)
    {
        fileName = Path.GetFileName(fileName);
        string librarySubFolder = GetCorrectLibrarySubfolderPathForAssetType(fileName);
        fileName = TofuPath.Combine(librarySubFolder, fileName);

        string extension = GetTofuAssetExtensionForAsset(fileName);
        if (fileName.EndsWith(extension) == false)
        {
            fileName = fileName + extension;
        }

        fileName = AssetPathConverter.ToProjectRelativePath(fileName);

        return fileName;
    }


    public static string ModelToMeshFileName(string fileName, int meshIndex)
    {
        return fileName + "_" + meshIndex + ".tofumesh";
    }


    public static bool IsLibraryPath(string path)
    {
        if (path.Contains(LibraryString1, StringComparison.OrdinalIgnoreCase) ||
            path.Contains(LibraryString2, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public static bool IsAssetsPath(string path) => IsLibraryPath(path) == false;

    public static string MeshToModelFileName(this string fileName)
    {
        string name = fileName.Remove(fileName.LastIndexOf("_")); // removes _0.tofumesh
        return name;
    }

    private static string GetCorrectLibrarySubfolderPathForAssetType(this string fileName)
    {
        string path = "";
        if (IsFileModel(fileName))
        {
            path = Folders.ModelsInLibrary;
        }

        if (IsFileMesh(fileName))
        {
            path = Folders.MeshesInLibrary;
        }

        if (IsFileTexture(fileName))
        {
            path = Folders.TexturesInLibrary;
        }

        if (IsFileMaterial(fileName))
        {
            path = Folders.MaterialsInLibrary;
        }

        if (IsFileTemporaryMisc(fileName))
        {
            path = Folders.TempInLibrary;
        }

        return path;
    }

    private static string GetTofuAssetExtensionForAsset(this string fileName)
    {
        string extension = "";
        if (IsFileModel(fileName))
        {
            extension = ".tofumodel";
        }

        if (IsFileTexture(fileName))
        {
            extension = ".tofutexture";
        }

        if (IsFileMaterial(fileName))
        {
            extension = ".tofumaterial";
        }

        return extension;
    }

    public static bool IsAssetImportParametersFile(string fileName)
    {
        return fileName.EndsWith(".importparameters", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileModel(string fileName)
    {
        return fileName.EndsWith(".tofumodel", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".obj", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileMesh(string fileName)
    {
        return fileName.EndsWith(".tofumesh", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileTexture(string fileName)
    {
        return fileName.EndsWith(".tofutexture", StringComparison.OrdinalIgnoreCase) ||
               (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsFileScene(string fileName)
    {
        return fileName.EndsWith(".scene", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileTemporaryMisc(string fileName)
    {
        return fileName.EndsWith(".temp", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileMaterial(string fileName)
    {
        return fileName.EndsWith(".tofumaterial", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".mat", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileShader(string fileName)
    {
        return fileName.EndsWith(".glsl", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileScript(string fileName)
    {
        return fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFilePrefab(string fileName)
    {
        return fileName.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
    }
    
    public static string ValidateAssetPath(ref string assetPath)
    {
        assetPath = ValidateAssetPath(assetPath);
        return assetPath;
    }
    
    // in case we want to cache
    public static bool Exists(string path) => File.Exists(path);

    public static string ValidateAssetPath(string assetPath)
    {
        assetPath = assetPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        // assetPath = assetPath.Replace(" ", "\\ ");

        // bool isValid = Exists(assetPath);
        // if (isValid) return assetPath;
        var existsInAssetFolder = File.Exists(TofuPath.Combine(Folders.Assets, assetPath));
        if (existsInAssetFolder)
        {
            assetPath = TofuPath.CombineRelativeTo(TofuPath.PathScope.Assets,Folders.Assets, assetPath);
        }
        else
        {
            assetPath = Folders.GetPathRelativeToProjectFolder(assetPath);
        }

        if (AssetPathExtensions.Exists(assetPath) == false)
        {
            var assetPathInAssetsFolder = TofuPath.Combine("Assets", assetPath);
            if (AssetPathExtensions.Exists(assetPathInAssetsFolder))
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