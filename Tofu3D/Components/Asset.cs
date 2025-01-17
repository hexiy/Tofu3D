[Serializable]
public abstract class Asset<T> : AssetBase where T : Asset<T> //, new()
{
    // public T CreateRuntimeCopy()
    // {
    //     string tempFileName =
    //         Folders.GetPathRelativeToProjectFolder(
    //             TofuPath.Combine(Folders.TempInLibrary, Random.Range(0, 100_000_000).ToString()) + ".temp");
    //     Tofu.AssetLoadManager.Save<T>(tempFileName, asset: (T)this);
    //     T runtimeCopy =
    //         Tofu.AssetLoadManager.Load<T>(tempFileName, null, false, isRuntimeCopy: true);
    //     runtimeCopy.SetAsRuntimeAsset();
    //     runtimeCopy.PathToAssetInLibrary = tempFileName;
    //     // File.Delete(tempFileName);
    //     Debug.Log("Created new copy of asset");
    //
    //     return runtimeCopy;
    // }
    public T? Clone()
    {
        var memberwiseClone = MemberwiseClone();
        var clone = (T)memberwiseClone;
        return clone as T;
    }
}