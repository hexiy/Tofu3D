namespace Tofu3D;

public abstract class AssetImporter<T> : IAssetImporter where T : Asset<T>
{
    public abstract T ImportAsset(AssetImportParameters<T>? assetImportParameters);
}