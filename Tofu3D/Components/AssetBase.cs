[Serializable]
public abstract class AssetBase
{
    [Hide]
    public string PathToRawAsset = "";
    [Hide]
    public string PathToAssetInLibrary=>PathToRawAsset.GetPathOfAssetInLibrayFromSourceAssetPathOrName();
    [XmlIgnore]
    public RuntimeAssetHandle RuntimeAssetHandle { get; set; }

    // private Asset()
    // {
    // }

    public static implicit operator bool(AssetBase instance)
    {
        if (instance == null)
        {
            return false;
        }

        return true;
    }
}