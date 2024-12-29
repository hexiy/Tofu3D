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

        byte[] pixels = new byte[4 * image.Width * image.Height];
        image.Frames[0].CopyPixelDataTo(pixels);
        image.Dispose();


        if (importParameters != null && importParameters.BlackIsTransparency)
        {
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte red = pixels[i];
                byte green = pixels[i + 1];
                byte blue = pixels[i + 2];
                if (red == green && red == blue)
                {
                    // Calculate the blackness by taking the average of the RGB channels.
                    float blackAmount = ((red + green + blue) / (3.0f * 255.0f));

                    // Set the alpha channel based on the amount of blackness
                    pixels[i + 3] = (byte)(blackAmount * 255);
                }
            }
        }

        Asset_Texture assetTexture = new Asset_Texture()
            { Pixels = pixels, TextureSize = imageSize, PathToRawAsset = path };
        
        Serializer.SaveAssetJSON<Asset_Texture>(path.GetPathOfAssetInLibrayFromSourceAssetPathOrName(),
            assetTexture);

        return assetTexture;
    }
}