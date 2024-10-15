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
        AssetImportParameters_Texture importParameters = assetImportParameters as AssetImportParameters_Texture;
        string path = assetImportParameters.PathToSourceAsset;
        if (File.Exists(path) == false)
        {
            path = Folders.GetResourcePath("purple.png");
        }
        
        var imageSize = Vector2.Zero;

        var image = Image.Load<Rgba32>(path);
        imageSize = new Vector2(image.Width, image.Height);

        var pixels = new byte[4 * image.Width * image.Height];
        image.Frames[0].CopyPixelDataTo(pixels);
        image.Dispose();

        Asset_Texture assetTexture = new Asset_Texture()
            { Pixels = pixels, TextureSize = imageSize, PathToRawAsset = path };
        QuickSerializer.SaveFileJSON<Asset_Texture>(path.GetPathOfAssetInLibrayFromSourceAssetPathOrName(), assetTexture);

        return assetTexture;
    }
}