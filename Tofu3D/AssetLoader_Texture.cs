using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tofu3D;

public class AssetLoader_Texture : AssetLoader<Asset_Texture, RuntimeTexture>
{
    public override RuntimeTexture LoadAsset(AssetLoadParameters<RuntimeTexture>? assetLoadParameters)
    {
        AssetLoadParameters_Texture loadParameters = assetLoadParameters as AssetLoadParameters_Texture;
        string path = loadParameters.PathToAsset;

        Asset_Texture assetTexture = QuickSerializer.ReadFileBinary<Asset_Texture>(path);
        
        var textureId = GL.GenTexture();
        TextureHelper.BindTexture(textureId);
        var textureTarget = TextureTarget.Texture2D;

        GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, (int)assetTexture.TextureSize.X,
            (int)assetTexture.TextureSize.Y, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, assetTexture.Pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)loadParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)loadParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)loadParameters.WrapMode);
        // GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)loadSettings.FilterMode);
        // GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)loadSettings.FilterMode);

        // crashes the engine on macos
        if (OperatingSystem.IsWindows)
        {
            GL.TextureParameter(textureId, TextureParameterName.TextureMinFilter, (int)loadParameters.FilterMode + 257);
            GL.TextureParameter(textureId, TextureParameterName.TextureLodBias, -0.4f);
        }

        ImGuiController.CheckGlError("texture load");

        RuntimeTexture runtimeTexture = new()
        {
            Size = assetTexture.TextureSize
            //     Size = imageSize,
            //     Loaded = true,
            //     PathToRawAsset = path.FromRawAssetFileNameToPathOfAssetInLibrary(),
        };
        runtimeTexture.TextureId = textureId;

        return runtimeTexture;
    }
}