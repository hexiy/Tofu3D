using System.IO;

namespace Tofu3D;

public class AssetLoader_Texture : AssetLoader<RuntimeTexture>
{
    public override RuntimeTexture LoadAsset(AssetLoadParameters<RuntimeTexture>? assetLoadParameters)
    {
        AssetLoadParameters_Texture loadParameters = assetLoadParameters as AssetLoadParameters_Texture;
        string path = loadParameters.PathToAssetInLibrary;

        if (File.Exists(AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path)))
        {
            path = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path);
        }

        Asset_Texture assetTexture = Serializer.ReadAssetJSON<Asset_Texture>(path);


        if (assetTexture.AtlasPath == null)
        {
            Debug.LogError("No atlas path in texture asset");
            // throw new NullReferenceException("no atlas path");
        }

        RuntimeAtlasTexture runtimeAtlasTexture =
            Tofu.AssetLoadManager.Load<RuntimeAtlasTexture>(sourcePath: assetTexture.AtlasPath);
        RuntimeTexture runtimeTexture = new()
        {
            AtlasGLTextureId = runtimeAtlasTexture?.GLTextureId ?? 0,
            BoundingBoxInAtlas = assetTexture.BoundingBoxInAtlas,
            PathInLibraryFolder = assetTexture.PathInLibraryFolder,
            PathInAssetsFolder = assetTexture.PathInAssetsFolder,
        };

        return runtimeTexture;
    }
}