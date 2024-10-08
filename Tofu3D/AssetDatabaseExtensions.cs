using System.IO;

public static class AssetDatabaseExtensions
{
    public static string FromFileNameToPathToLibraryImportParameters(this string fileName)
    {
        return Path.Combine(Folders.Library,fileName + ".importParameters");
    }
    
    // from /Assets/car.obj to /Library/car.asset
    public static string FromRawAssetFileNameToPathOfAssetInLibrary(this string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return Path.Combine(Folders.Library,fileName + ".asset");
    }
}