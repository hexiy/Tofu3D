using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace Tofu3D;

public class AssetImporter_Material : AssetImporter<Asset_Material>
{
    private readonly XmlSerializer _xmlSerializer;


    public override Asset_Material ImportAsset(AssetImportParameters<Asset_Material> assetImportParameters)
    {
        AssetImportParameters_Material importParameters = assetImportParameters as AssetImportParameters_Material;


        Asset_Material material = QuickSerializer.ReadFileXML<Asset_Material>(assetImportParameters.PathToSourceAsset);
        // material.LoadTextures();
        // if (material.Shader != null)
        // {
            // material.InitShader();
        // }
        material.PathToRawAsset = assetImportParameters.PathToSourceAsset;
        string path = importParameters.PathToSourceAsset.FromRawAssetFileNameToPathOfAssetInLibrary();
        QuickSerializer.SaveFileJSON<Asset_Material>(path, material);

        // material.InitAssetRuntimeHandle(material.Vao);
        // material.IsValid = true;
        return material;
    }
}