using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TofuEngine;

public class AssetImporter_Texture : AssetImporter<Asset_Texture>
{
    public override Asset_Texture ImportAsset(AssetImportParameters<Asset_Texture> assetImportParameters)
    {
        AssetImportParameters_Texture importParameters = assetImportParameters as AssetImportParameters_Texture;
        string path = assetImportParameters.PathToSourceAsset;
        if (File.Exists(path) == false)
        {
            path = Folders.GetEditorResourcePath("purple.png");
        }

        Vector2 imageSize = Vector2.Zero;

        Image<Rgba32>? image = Image.Load<Rgba32>(path);
        int maxResolution = 1024;

        Vector2 newResolution = new Vector2(Mathf.ClampMax(image.Width, maxResolution),
            Mathf.ClampMax(image.Height, maxResolution));

        if (image.Width != newResolution.X || image.Height != newResolution.Y)
        {
            image.Mutate(x => x.Resize(newResolution.Xi, newResolution.Yi));
        }

        byte[] pixels = new byte[4 * newResolution.Xi * newResolution.Yi];
        image.Frames[0].CopyPixelDataTo(pixels);
        image.Dispose();

        return ImportAsset(importParameters, pixels, newResolution);
    }

    private Asset_Texture ImportAsset(AssetImportParameters_Texture importParameters, byte[] pixels, Vector2 imageSize)
    {
        string path = importParameters.PathToSourceAsset;

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

        string libraryPath = AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path);
        Asset_Texture assetTexture = new Asset_Texture()
        {
            Pixels = pixels, TextureSize = imageSize, PathInAssetsFolder = path,
            PathInLibraryFolder = libraryPath
        };
        assetTexture.AssetImportParameters = importParameters;
        
        Serializer.SaveAssetJSON<Asset_Texture>(
            AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path),
            assetTexture);

        return assetTexture;
    }
}