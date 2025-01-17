namespace Tofu3D;

public class AssetImporter_Material : AssetImporter<Asset_Material>
{
    public override Asset_Material ImportAsset(AssetImportParameters<Asset_Material> assetImportParameters)
    {
        AssetImportParameters_Material importParameters = assetImportParameters as AssetImportParameters_Material;

        Asset_Material material = Serializer.ReadAssetJSON<Asset_Material>(assetImportParameters.PathToSourceAsset);


        material.PathInAssetsFolder = assetImportParameters.PathToSourceAsset;
        string libraryPath = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(importParameters.PathToSourceAsset);
        material.PathInLibraryFolder = libraryPath;
        Serializer.SaveAssetJSON<Asset_Material>(libraryPath, material);

        // save the material back, i had a problem where i changed default value for shadwomap texture unity but the /assets/material was unchanged so shadowmap kept textureunit0...
        Serializer.SaveAssetJSON<Asset_Material>(assetImportParameters.PathToSourceAsset, material);

        return material;
    }
}