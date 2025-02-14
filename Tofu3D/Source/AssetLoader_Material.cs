using System.IO;

namespace TofuEngine;

public class AssetLoader_Material : AssetLoader<Asset_Material>
{
    public override Asset_Material LoadAsset(AssetLoadParameters<Asset_Material>? assetLoadParameters)
    {
        string path = assetLoadParameters.PathToAssetInLibrary;
        if (File.Exists(path) == false)
        {
            return null;
        }

        Asset_Material assetMaterial = Serializer.ReadFileJSON<Asset_Material>(path);

        assetMaterial.LoadTextures();
        if (assetMaterial.Shader != null)
        {
            assetMaterial.LoadShader();
        }

        // save the material back, i had a problem where i changed default value for shadwomap texture unity but the /assets/material was unchanged so shadowmap kept textureunit0...
        // QuickSerializer.SaveFileJSON<Asset_Material>(path, assetMaterial);
        // this ^ is now in assetimporter_material
        
        return assetMaterial;
    }
}