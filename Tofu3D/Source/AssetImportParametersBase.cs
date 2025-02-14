public abstract class AssetImportParametersBase
{
    /// <summary>
    /// Raw asset, .png, .obj, somewhere in /Assets/ folder
    /// </summary>
    [Hide]
    public string PathToSourceAsset;

    [Hide]
    public int AssetID => PathToSourceAsset.GetHashCode();
}