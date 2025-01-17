namespace Tofu3D;

public class AssetLoader_Model : AssetLoader<Asset_Model>
{
    public override Asset_Model LoadAsset(AssetLoadParameters<Asset_Model>? assetLoadParameters)
    {
        string modelAssetPath = assetLoadParameters.PathToAssetInLibrary;

        Asset_Model assetModel = Serializer.ReadFileJSON<Asset_Model>(modelAssetPath);

        return assetModel;
    }
}