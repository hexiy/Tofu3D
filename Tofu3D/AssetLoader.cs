namespace Tofu3D;

// loads .asset into runtime
public abstract class AssetLoader<T1,T2> : IAssetLoader where T1 : Asset<T1>
{
    public abstract T2 LoadAsset(AssetLoadParameters<T2>? assetLoadParameters);
    // public object LoadAsset(AssetLoadParameters<object>? assetLoadParameters) => throw new NotImplementedException();
}