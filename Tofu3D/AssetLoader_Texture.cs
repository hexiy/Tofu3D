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
            // Debug.LogError("No atlas path in texture asset");
            // throw new NullReferenceException("no atlas path");
        }


        RuntimeTexture runtimeTexture = new RuntimeTexture
        {
            BoundingBoxInAtlas = assetTexture.BoundingBoxInAtlas,
            IndexInAtlasTextureArray = assetTexture.IndexInAtlasTextureArray,
            PathInLibraryFolder = assetTexture.PathInLibraryFolder,
            PathInAssetsFolder = assetTexture.PathInAssetsFolder,
        };


        // if (loadParameters.LoadType.HasFlag(TextureLoadType.Standalone))
        // {
            // Debug.LogError("Standalone textures are disabled");
            // var standaloneGLTextureId = loadParameters.ExistingAsset?.StandaloneGLTextureId ?? GL.GenTexture();
            // TextureHelper.BindTexture(standaloneGLTextureId);
            // var textureTarget = TextureTarget.Texture2D;
            //
            // var internalFormat = PixelInternalFormat.Rgba;
            //
            // GL.TexImage2D(textureTarget, 0, internalFormat, (int)assetTexture.TextureSize.X,
            //     (int)assetTexture.TextureSize.Y, 0, PixelFormat.Rgba,
            //     PixelType.UnsignedByte, assetTexture.Pixels);
            //
            // TextureWrapMode wrapMode = TextureWrapMode.Repeat;
            // TextureFilterMode filterMode = TextureFilterMode.Point;
            //
            // GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)wrapMode);
            // GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)wrapMode);
            // GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)wrapMode);
            // GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)filterMode);
            // GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)filterMode);
            //
            // TofuGL.CheckGlError("standalone texture load");
            //
            // runtimeTexture.StandaloneGLTextureId = standaloneGLTextureId;
        // }

        if (assetTexture.AtlasPath != null)
        {
            // Asset_TextureAtlas assetTextureAtlas =
            // Serializer.ReadAssetJSON<Asset_TextureAtlas>(path: assetTexture.AtlasPath);
            // runtimeTexture.IndexInAtlasTextureArray = assetTextureAtlas?.IndexInTextureArray ?? 0;
        }


        return runtimeTexture;
    }
}