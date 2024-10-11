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
        fileName = fileName.ToLower();
        return fileName.EndsWith(".tofumodel") || fileName.EndsWith(".obj");
    }

    public static bool IsFileMesh(string fileName)
    {
        fileName = fileName.ToLower();
        return fileName.EndsWith(".tofumesh");
    }

    public static bool IsFileTexture(string fileName)
    {
        fileName = fileName.ToLower();
        return fileName.EndsWith(".tofutexture") ||
               (fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg") ||
                fileName.EndsWith(".bmp"));
    }

    public static bool IsFileMaterial(string fileName)
    {
        fileName = fileName.ToLower();
        return fileName.EndsWith(".tofumaterial") || fileName.EndsWith(".mat");
    }

    public static bool IsFileShader(string fileName)
    {
        fileName = fileName.ToLower();
        return fileName.EndsWith(".glsl");
    }

    public static bool IsFilePrefab(string fileName)
    {
        fileName = fileName.ToLower();
        return fileName.EndsWith(".prefab");
    }
}