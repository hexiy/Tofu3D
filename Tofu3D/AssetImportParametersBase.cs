public abstract class AssetImportParametersBase
{
    /// <summary>
    /// Raw asset, .png, .obj, somewhere in /Assets/ folder
    /// </summary>
    public string PathToSourceAsset;

    public int AssetID => PathToSourceAsset.GetHashCode();
}