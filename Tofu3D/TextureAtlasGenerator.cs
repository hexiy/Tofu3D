using System.Drawing;
using System.IO;
using System.Linq;
using LibNoise.Renderer;
using OpenTK.Mathematics;
using RectpackSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Tofu3D;

public static class TextureAtlasGenerator
{
    private const int AtlasWidth = 4096;

    public static void GenerateAtlasesForTextures(List<Asset_Texture> textures)
    {
        textures.Sort();

        PackingRectangle[] allRectangles = new PackingRectangle[textures.Count];

        List<PackingRectangle> currentRectangles = new List<PackingRectangle>();
        List<PackingRectangle[]> rectangleGroups = new List<PackingRectangle[]>();

        Vector2 pixelsLeft = new Vector2(AtlasWidth, AtlasWidth);
        for (int i = 0; i < allRectangles.Length; i++)
        {
            pixelsLeft -= textures[i].TextureSize;

            // if (currentRectangles.Count == 0)
            // {
            //     Debug.LogError("texture didn't fit in this atlas");
            //     continue;
            // }

            PackingRectangle[] rectsOld = currentRectangles.ToArray();
            PackingRectangle[] rects = currentRectangles.ToArray();
            bool packed = false;
            try
            {
                RectanglePacker.Pack(rects, out PackingRectangle bounds, maxBoundsHeight: AtlasWidth,
                    maxBoundsWidth: AtlasWidth);
                packed = true;
            }
            catch (Exception ex)
            {
                packed = false;
            }

            if (packed == false)
            {
                currentRectangles = rectsOld.ToList();

                rectangleGroups.Add(currentRectangles.ToArray());
                i--;
                currentRectangles.Clear();
                pixelsLeft = new Vector2(AtlasWidth, AtlasWidth);
                continue;
            }
            else
            {
                currentRectangles = rects.ToList();

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
            TextureAtlasMember[] _textureAtlasMembers = new TextureAtlasMember[textureRectangles.Length];

            string atlasPath = Path.Combine(Folders.TextureAtlasesInLibrary,
                $"atlas_{atlIndex}.tofutextureatlas");

            byte[] atlasPixels = new byte[4 * AtlasWidth * AtlasWidth];
            for (int i = 0; i < textureRectangles.Length; i++)
            {
                PackingRectangle rectangle = textureRectangles[i];
                Asset_Texture texture = textures[rectangle.Id];
                texture.OnDeserialized(); // make sure the pixels are decompressed

                Vector4 box = new Vector4(rectangle.X, rectangle.Y, rectangle.X + rectangle.Width,
                    rectangle.Y + rectangle.Height);

                _textureAtlasMembers[i] = new TextureAtlasMember()
                {
                    BoundingBox = box,
                    PathToTextureInLibrary = texture.PathInAssetsFolder,
                };


                texture.AtlasPath = atlasPath;
                texture.BoundingBoxInAtlas = box;

                Tofu.AssetLoadManager.Save<Asset_Texture>(texture.PathInLibraryFolder, texture);

                int textureWidth = (int)rectangle.Width;
                int textureHeight = (int)rectangle.Height;

                for (int y = 0; y < textureHeight; y++)
                {
                    for (int x = 0; x < textureWidth; x++)
                    {
                        int atlasX = (int)rectangle.X + x;
                        int atlasY = (int)rectangle.Y + y;

                        int indexInAtlasPixels = (atlasY * AtlasWidth + atlasX) * 4;
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
                {
                    PathToSourceAsset = atlasPath
                };


            ///////////////////////////////////////////////////////////
            using var image =
                Image.LoadPixelData<Rgba32>(atlasPixels, AtlasWidth, AtlasWidth);
            // image.Mutate(x =>
            // {
            //     x.Flip(FlipMode.Vertical);
            // });
            byte[] pixels = new byte[AtlasWidth * AtlasWidth * 4];
            image.CopyPixelDataTo(pixels);


            string path = Path.Combine(Folders.TextureAtlasesInLibrary, $"atlas_{atlIndex}.png");
            image.SaveAsPng(path);
            ///////////////////////////////////////////////////////////


            atlasPixels = Compression.Compress(atlasPixels);
            Asset_TextureAtlas atlasTexture = new Asset_TextureAtlas()
            {
                Pixels = atlasPixels, TextureSize = new Vector2(AtlasWidth, AtlasWidth), PathInLibraryFolder = atlasPath
            };

            Serializer.SaveAssetJSON<Asset_TextureAtlas>(atlasPath, atlasTexture);

            atlasTexture.TextureAtlasMembers = _textureAtlasMembers;

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