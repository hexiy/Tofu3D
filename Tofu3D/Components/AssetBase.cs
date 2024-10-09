[Serializable]
public abstract class AssetBase
{
    public string PathToRawAsset = "";
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