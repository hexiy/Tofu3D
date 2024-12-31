namespace Tofu3D;

// loads .asset into runtime
public abstract class AssetLoader<T1> : IAssetLoader where T1 : class
{
    public abstract T1 LoadAsset(AssetLoadParameters<T1>? assetLoadParameters);
    // public object LoadAsset(AssetLoadParameters<object>? assetLoadParameters) => throw new NotImplementedException();
}