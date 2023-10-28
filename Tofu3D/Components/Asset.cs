namespace Tofu3D;

[Serializable]
public abstract class Asset<T> : AssetBase where T : Asset<T> //, new()
{
    public void InitAssetRuntimeHandle(int id)
    {
        Handle = new AssetHandle { Id = id, AssetType = typeof(T) };
    }
}