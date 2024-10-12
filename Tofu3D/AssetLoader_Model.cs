using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace Tofu3D;

public class AssetLoader_Model : AssetLoader<Asset_Model, Asset_Model>
{
    public override Asset_Model LoadAsset(AssetLoadParameters<Asset_Model>? assetLoadParameters)
    {
        string modelAssetPath = assetLoadParameters.PathToAsset;

        Asset_Model assetModel = QuickSerializer.ReadFileJSON<Asset_Model>(modelAssetPath);

        return assetModel;
    }
}