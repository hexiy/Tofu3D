using System.IO;

namespace Tofu3D;

public class AssetLoader_AssetTexture : AssetLoader<Asset_Texture>
{
    public override Asset_Texture LoadAsset(AssetLoadParameters<Asset_Texture>? assetLoadParameters)
    {
        Asset_Texture assetTexture = Serializer.ReadAssetJSON<Asset_Texture>(assetLoadParameters.PathToAssetInLibrary);

        return assetTexture;
    }
}