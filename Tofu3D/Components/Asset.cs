using System.IO;

[Serializable]
public abstract class Asset<T> : AssetBase where T : Asset<T> //, new()
{
    public void InitAssetRuntimeHandle(int id)
    {
        RuntimeAssetHandle = new RuntimeAssetHandle { Id = id, AssetType = typeof(T) };
    }

    public T CreateRuntimeCopy()
    {
        // save this temporarily

        string tempFileName = Path.Combine(Folders.TempInLibrary, Random.Range(0, 100_000_000).ToString())+".temp";
        Tofu.AssetLoadManager.Save<T>(tempFileName, asset: (T)this);
        T runtimeCopy =
            Tofu.AssetLoadManager.Load<T>(tempFileName, null, false, creatingRuntimeCopy: true);
        runtimeCopy.SetAsRuntimeAsset();
        
        File.Delete(tempFileName);
        Debug.Log("Created new copy of material");

        return runtimeCopy;
    }
}