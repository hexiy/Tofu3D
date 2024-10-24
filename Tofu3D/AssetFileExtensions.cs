using System.IO;
using System.Text;

public static class AssetFileExtensions
{
    public static string GetPathOfImportParametersOfSourceAssetFile(this string sourceFilePath)
    {
        return sourceFilePath + ".importparameters";
    }
    // from /Assets/car.obj to /Library/car.asset
    public static string GetPathOfAssetInLibrayFromSourceAssetPathOrName(this string fileName)
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