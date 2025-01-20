using System.Drawing;
using System.IO;
using System.Linq;
using LibNoise.Renderer;
using RectpackSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Tofu3D;

public static class TextureAtlasGenerator
{
    public static void GenerateAtlasesForTextures(List<Asset_Texture> textures)
    {
        textures.Sort();

        PackingRectangle[] allRectangles = new PackingRectangle[textures.Count];

        List<PackingRectangle> currentRectangles = new List<PackingRectangle>();
        List<PackingRectangle[]> rectangleGroups = new List<PackingRectangle[]>();

        Vector2 pixelsLeft = new Vector2(4096, 4096);
        for (int i = 0; i < allRectangles.Length; i++)
        {
            pixelsLeft -= textures[i].TextureSize;
            if (pixelsLeft.X < 0 || pixelsLeft.Y < 0)
            {
                if (currentRectangles.Count == 0)
                {
                    Debug.LogError("texture didn't fit in this atlas");
                    continue;
                }

                PackingRectangle[] rects = currentRectangles.ToArray();
                RectanglePacker.Pack(rects, out PackingRectangle bounds, maxBoundsHeight: 4096,
                    maxBoundsWidth: 4096);
                currentRectangles = rects.ToList();

                rectangleGroups.Add(currentRectangles.ToArray());
                i--;
                currentRectangles.Clear();
                pixelsLeft = new Vector2(4096, 4096);
                continue;
            }
            else
            {
                PackingRectangle rectangle = new PackingRectangle(x: 0, y: 0, width: (uint)textures[i].TextureSize.X,
                    height: (uint)textures[i].TextureSize.Y, id: i);

                currentRectangles.Add(rectangle);
            }
        }

        if (currentRectangles.Count > 0)
        {
            rectangleGroups.Add(currentRectangles.ToArray());
            currentRectangles.Clear();
        }

        int atlIndex = 0;
        foreach (PackingRectangle[] textureRectangles in rectangleGroups)
        {
            byte[] atlasPixels = new byte[4 * 4096 * 4096];
            for (int i = 0; i < textureRectangles.Length; i++)
            {
                PackingRectangle rectangle = textureRectangles[i];
                Asset_Texture texture = textures[rectangle.Id];
                texture.OnDeserialized(); // make sure the pixels are decompressed

                int textureWidth = (int)rectangle.Width;
                int textureHeight = (int)rectangle.Height;

                for (int y = 0; y < textureHeight; y++)
                {
                    for (int x = 0; x < textureWidth; x++)
                    {
                        int atlasX = (int)rectangle.X + x;
                        int atlasY = (int)rectangle.Y + y;

                        int indexInAtlasPixels = (atlasY * 4096 + atlasX) * 4;
                        int indexInTexturePixels = (y * textureWidth + x) * 4;
                        try
                        {
                            atlasPixels[indexInAtlasPixels] = texture.Pixels[indexInTexturePixels]; // r
                            atlasPixels[indexInAtlasPixels + 1] = texture.Pixels[indexInTexturePixels + 1]; // g
                            atlasPixels[indexInAtlasPixels + 2] = texture.Pixels[indexInTexturePixels + 2]; // b
                            atlasPixels[indexInAtlasPixels + 3] = texture.Pixels[indexInTexturePixels + 3]; // a
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex.Message);
                        }
                    }
                }
            }

            AssetImporter_Texture assetImporterTexture = new AssetImporter_Texture();
            AssetImportParameters_Texture assetImportParametersTexture =
                new AssetImportParameters_Texture()
                    { PathToSourceAsset = Path.Combine(Folders.Assets, $"atlas_{atlIndex}.tofutextureatlas") };

            atlasPixels = Compression.Compress(atlasPixels);
            Asset_TextureAtlas atlas = assetImporterTexture.ImportAsset(assetImportParametersTexture, atlasPixels,
                imageSize: new Vector2(4096, 4096)) as Asset_TextureAtlas;

            atlIndex++;
        }


        // int atlasesCount = (int)MathF.Ceiling((float)bounds.Area / 4096f);
        // Asset_TextureAtlas[] atlases = new Asset_TextureAtlas[atlasesCount];
        //
        // for (int atlasIndex = 0; atlasIndex < atlasesCount; atlasIndex++)
        // {
        //     
        // }


        // All the rectangles in the array were assigned X and Y values. Bounds contains the width and height of the bin.
    }

    // private static Asset_TextureAtlas CreateTextureAtlas(AssetImportParameters_TextureAtlas assetImportParameters)
    // {
    //     Asset_TextureAtlas atlas = new Asset_TextureAtlas();
    // }
}