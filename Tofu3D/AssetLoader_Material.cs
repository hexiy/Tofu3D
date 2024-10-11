using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace Tofu3D;

public class AssetLoader_Material : AssetLoader<Asset_Material, Asset_Material>
{
    public override Asset_Material LoadAsset(AssetLoadParameters<Asset_Material>? assetLoadParameters)
    {
        string path = assetLoadParameters.PathToAsset;

        Asset_Material assetMaterial = QuickSerializer.ReadFileJSON<Asset_Material>(path);
        
        assetMaterial.LoadTextures();
        if (assetMaterial.Shader != null)
        {
            assetMaterial.InitShader();
        }
        
        assetMaterial.InitAssetRuntimeHandle(assetMaterial.Vao);
        // material.IsValid = true;
        return assetMaterial;
    }

}