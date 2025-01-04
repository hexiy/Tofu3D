using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

        var pathOfImportParametersOfSourceAssetFile =
            AssetPathExtensions.GetPathOfImportParametersOfSourceAssetFile(assetTexture.PathInAssetsFolder);
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

        var textureId = loadParameters.ExistingAsset?.TextureId ?? GL.GenTexture();
        TextureHelper.BindTexture(textureId);
        var textureTarget = TextureTarget.Texture2D;

        var internalFormat = importParameters.IsSrgb ? PixelInternalFormat.SrgbAlpha : PixelInternalFormat.Rgba;

        GL.TexImage2D(textureTarget, 0, internalFormat, (int)assetTexture.TextureSize.X,
            (int)assetTexture.TextureSize.Y, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, assetTexture.Pixels);

        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)importParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)importParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)importParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)importParameters.FilterMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)importParameters.FilterMode);

        ImGuiController.CheckGlError("texture load");

        RuntimeTexture runtimeTexture = new()
        {
            Size = assetTexture.TextureSize,
            PathInLibraryFolder = assetTexture.PathInLibraryFolder,
            PathInAssetsFolder = assetTexture.PathInAssetsFolder,
        };
        runtimeTexture.TextureId = textureId;

        return runtimeTexture;
    }
}