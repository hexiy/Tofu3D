namespace TofuEngine;

public class AssetLoader_Model : AssetLoader<Asset_Model>
{
    public override Asset_Model LoadAsset(AssetLoadParameters<Asset_Model>? assetLoadParameters)
    {
        AssetLoadParameters_Model loadParameters = assetLoadParameters as AssetLoadParameters_Model;
        string modelAssetPath = assetLoadParameters.PathToAssetInLibrary;

        Asset_Model assetModel = Serializer.ReadFileJSON<Asset_Model>(modelAssetPath);
        if (assetModel == null)
        {
            Debug.LogError($"assetModel is null:{modelAssetPath}");
            return null;
        }

        assetModel.AssetLoadParameters = loadParameters;
        return assetModel;
    }
}