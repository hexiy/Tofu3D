[Serializable]
public abstract class AssetBase
{
    [Hide]
    public string Path = "";

    [Hide]
    public bool IsRuntimeCopy = false;

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