using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tofu3D;

public class AssetImporter_Texture : AssetImporter<Asset_Texture>
{

    public override Asset_Texture ImportAsset(AssetImportParameters<Asset_Texture> assetImportParameters)
    {
        var id = GL.GenTexture();
        AssetImportParameters_Texture importParameters = assetImportParameters as AssetImportParameters_Texture;
        string path = assetImportParameters.PathToSourceAsset;
        if (File.Exists(path) == false)
        {
            path = Folders.GetResourcePath("purple.png");
        }

        TextureHelper.BindTexture(id);

        var imageSize = Vector2.Zero;

        var image = Image.Load<Rgba32>(path);
        imageSize = new Vector2(image.Width, image.Height);

        var pixels = new byte[4 * image.Width * image.Height];
        image.Frames[0].CopyPixelDataTo(pixels);
        image.Dispose();

        var textureTarget = TextureTarget.Texture2D;

        GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)importParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)importParameters.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)importParameters.WrapMode);
        // GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)loadSettings.FilterMode);
        // GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)loadSettings.FilterMode);

        // crashes the engine on macos
        if (OperatingSystem.IsWindows)
        {
            GL.TextureParameter(id, TextureParameterName.TextureMinFilter, (int)importParameters.FilterMode + 257);
            GL.TextureParameter(id, TextureParameterName.TextureLodBias, -0.4f);
        }

        ImGuiController.CheckGlError("texture load");

        Asset_Texture texture = new()
        {
            Size = imageSize,
            Loaded = true,
            Path = path.FromRawAssetFileNameToPathOfAssetInLibrary(),
        };
        texture.InitAssetRuntimeHandle(id);

        // QuickSerializer.SaveFile<Asset_Texture>(texture.Path, texture);
        return texture;
    }
}