[Serializable]
public abstract class Asset<T> : AssetBase where T : Asset<T> //, new()
{
    public void InitAssetRuntimeHandle(int id)
    {
        RuntimeAssetHandle = new RuntimeAssetHandle { Id = id, AssetType = typeof(T) };
    }
}