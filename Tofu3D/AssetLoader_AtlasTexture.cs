using System.IO;

namespace Tofu3D;

public class AssetLoader_AtlasTexture : AssetLoader<RuntimeAtlasTexture>
{
    public override RuntimeAtlasTexture LoadAsset(AssetLoadParameters<RuntimeAtlasTexture>? assetLoadParameters)
    {
        // AssetLoadParameters_Texture loadParameters = assetLoadParameters as AssetLoadParameters_Texture;
        var loadParameters = assetLoadParameters;
        string path = loadParameters.PathToAssetInLibrary;

        if (File.Exists(AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path)))
        {
            path = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path);
        }

        Asset_TextureAtlas assetTextureAtlas = Serializer.ReadAssetJSON<Asset_TextureAtlas>(path);

        var pathOfImportParametersOfSourceAssetFile =
            AssetPathExtensions.GetPathOfImportParametersOfSourceAssetFile(assetTextureAtlas.PathInAssetsFolder);
        AssetImportParameters_Texture importParameters;

        if (File.Exists(pathOfImportParametersOfSourceAssetFile))
        {
            importParameters =
                Serializer.ReadFileJSON<AssetImportParameters_Texture>(pathOfImportParametersOfSourceAssetFile);
        }
        else
        {
            importParameters = new AssetImportParameters_Texture();
        }

        var textureId = loadParameters.ExistingAsset?.GLTextureId ?? GL.GenTexture();
        TextureHelper.BindTexture(textureId);
        var textureTarget = TextureTarget.Texture2D;

        var internalFormat = importParameters.IsSrgb ? PixelInternalFormat.SrgbAlpha : PixelInternalFormat.Rgba;

        byte[] pixels = new byte[(int)assetTextureAtlas.TextureSize.X / 2 * (int)assetTextureAtlas.TextureSize.Y / 2];
        GL.TexImage2D(textureTarget, 0, internalFormat, (int)assetTextureAtlas.TextureSize.X / 2,
            (int)assetTextureAtlas.TextureSize.Y / 2, 0, PixelFormat.Rgba,
            // PixelType.UnsignedByte, assetTextureAtlas.Pixels);
            PixelType.UnsignedByte, pixels);

        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)importParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)importParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)importParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)importParameters.FilterMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)importParameters.FilterMode);

        TofuGL.CheckGlError("atlas texture load");


        RuntimeAtlasTexture runtimeAtlasTexture = new()
        {
            PathInLibraryFolder = assetTextureAtlas.PathInLibraryFolder,
            PathInAssetsFolder = assetTextureAtlas.PathInAssetsFolder,
        };

        return runtimeAtlasTexture;
    }
}