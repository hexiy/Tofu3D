[Serializable]
public abstract class Asset<T> : AssetBase where T : Asset<T> //, new()
{
    public void InitAssetRuntimeHandle(int id)
    {
        RuntimeAssetHandle = new RuntimeAssetHandle { Id = id, AssetType = typeof(T) };
    }

    public T CreateRuntimeCopy()
    {
        T runtimeCopy =
            Tofu.AssetLoadManager.Load<T>(this.PathToRawAsset, null, false, isRuntimeCopy: true);
        runtimeCopy.SetAsRuntimeAsset();
        Debug.Log("Created new copy of material");

        return runtimeCopy;
    }
}