using System.IO;

public static class AssetFileExtensions
{
    public static string FromFileNameToPathToLibraryImportParameters(this string fileName)
    {
        string librarySubFolder = GetCorrectLibrarySubfolderPathForAssetType(fileName);

        return Path.Combine(librarySubFolder, fileName + ".importParameters");
    }

    // from /Assets/car.obj to /Library/car.asset
    public static string FromRawAssetFileNameToPathOfAssetInLibrary(this string fileName)
    {
        fileName = Path.GetFileName(fileName);
        string librarySubFolder = GetCorrectLibrarySubfolderPathForAssetType(fileName);
        fileName = Path.Combine(librarySubFolder, fileName);

        string extension = GetTofuAssetExtensionForAsset(fileName);
        if (fileName.EndsWith(extension) == false)
        {
            fileName = fileName + extension;
        }

        return fileName;
    }


    public static string ToMeshAssetFileName(this string fileName, int meshIndex)
    {
        return fileName + "_" + meshIndex + ".tofumesh";
    }

    private static string GetCorrectLibrarySubfolderPathForAssetType(this string fileName)
    {
        fileName = fileName.ToLower();

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


        return path;
    }

    private static string GetTofuAssetExtensionForAsset(this string fileName)
    {
        fileName = fileName.ToLower();

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

    public static bool IsFileModel(string fileName)
    {
        return fileName.EndsWith(".tofumodel", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".obj", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileMesh(string fileName)
    {
        return fileName.EndsWith(".tofumesh", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileTexture(string fileName)
    {
        return fileName.EndsWith(".tofutexture", StringComparison.OrdinalIgnoreCase) ||
               (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsFileMaterial(string fileName)
    {
        return fileName.EndsWith(".tofumaterial", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".mat", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFileShader(string fileName)
    {
        return fileName.EndsWith(".glsl", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFilePrefab(string fileName)
    {
        return fileName.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
    }
}