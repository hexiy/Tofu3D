[Serializable]
public abstract class AssetBase : IHasPath
{
    public AssetImportParametersBase? AssetImportParameters;
    public AssetLoadParametersBase? AssetLoadParameters;

    [XmlIgnore]
    [JsonIgnore]
    public bool CanBeSerialized = true;

    [Hide]
    public string? PathInAssetsFolder { get; set; } = null;

    [Hide]
    public string? PathInLibraryFolder { get; set; } = null;

    [Hide]
    public string? AnyPath
    {
        get { return PathInAssetsFolder ?? PathInLibraryFolder; }
    }

    [Hide]
    public bool IsRuntimeCopy = false;

    [Hide]
    public bool DataIsCompressed = false;

    public void SetAsRuntimeAsset()
    {
        IsRuntimeCopy = true;
    }

    public static implicit operator bool(AssetBase instance)
    {
        if (instance == null)
        {
            return false;
        }

        return true;
    }

    public virtual void OnDeserialized()
    {
    }

    public virtual void BeforeSerialized()
    {
    }
}