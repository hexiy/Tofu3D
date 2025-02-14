/*namespace Tofu3D;

/// <summary>
/// Holds asset files
/// </summary>
public class AssetFileCache
{
    public Dictionary<int, AssetBase> Assets { get; private set; } =
        new Dictionary<int, AssetBase>(); // int is (raw asset)path hashcode


    public void AddAsset(AssetBase assetBase)
    {
        int id = assetBase.PathInLibraryFolder.GetHashCode();

        Assets[id] = assetBase;
    }

    public T? GetAsset<T>(string pathInLibraryFolder, out T assetBase) where T : AssetBase
    {
        int id = pathInLibraryFolder.GetHashCode();
        if (Assets.TryGetValue(id, out AssetBase asset))
        {
            assetBase = asset as T;
            return assetBase as T;
        }
        else
        {
            assetBase = Serializer.ReadAssetJSON<T>(pathInLibraryFolder);
            Assets[id] = assetBase;
            return assetBase as T;
        }
    }
}*/