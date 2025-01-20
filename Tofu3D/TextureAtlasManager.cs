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

public static class TextureAtlasManager
{
    private static int AtlasWidth = -1;
    public static int _glTextureArrayId;
    private static int _atlasesCount;

    private static void SetupTextureArray()
    {
        _glTextureArrayId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DArray, _glTextureArrayId);

        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, AtlasWidth, AtlasWidth, _atlasesCount);

        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);
    }

    public static void GenerateAtlasesForTextures(List<Asset_Texture> textures)
    {


        if (AtlasWidth == -1)
        {
            AtlasWidth = GL.GetInteger(GetPName.MaxTextureSize);
        }

        textures.Sort();

        PackingRectangle[] allRectangles = new PackingRectangle[textures.Count];

        List<PackingRectangle> currentRectangles = new List<PackingRectangle>();
        List<PackingRectangle[]> atlasesRectangles = new List<PackingRectangle[]>();

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
                RectanglePacker.Pack(rects, out PackingRectangle bounds, maxBoundsHeight: (uint)AtlasWidth,
                    maxBoundsWidth: (uint)AtlasWidth);
                packed = true;
            }
            catch (Exception ex)
            {
                packed = false;
            }

            if (packed == false)
            {
                currentRectangles = rectsOld.ToList();

                atlasesRectangles.Add(currentRectangles.ToArray());
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
            atlasesRectangles.Add(currentRectangles.ToArray());
            currentRectangles.Clear();
        }

        int atlasIndex = 0;

        _atlasesCount = atlasesRectangles.Count;
        SetupTextureArray();

        
        foreach (PackingRectangle[] textureRectangles in atlasesRectangles)
        {
            TextureAtlasMember[] _textureAtlasMembers = new TextureAtlasMember[textureRectangles.Length];

            string atlasPath = Path.Combine(Folders.TextureAtlasesInLibrary,
                $"atlas_{atlasIndex}.tofutextureatlas");

            byte[] atlasPixels = new byte[4 * AtlasWidth * AtlasWidth];
            for (int i = 0; i < textureRectangles.Length; i++)
            {
                PackingRectangle rectangle = textureRectangles[i];
                Asset_Texture texture = textures[rectangle.Id];
                texture.OnDeserialized(); // make sure the pixels are decompressed

                Vector4 box = new Vector4(rectangle.X, rectangle.Y, rectangle.X + rectangle.Width,
                    rectangle.Y + rectangle.Height);
                box = box / AtlasWidth;
                

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
            
            
            GL.TexSubImage3D(
                TextureTarget.Texture2DArray, // Target
                0, // Level (0 = base level)
                0, 0, atlasIndex, // x, y offsets, and layer index
                AtlasWidth, // Width of the texture
                AtlasWidth, // Height of the texture
                1, // Depth (1 = single layer)
                PixelFormat.Rgba, // Format of input data
                PixelType.UnsignedByte, // Type of pixel data
                atlasPixels // Pointer to image data
            );

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


            string path = Path.Combine(Folders.TextureAtlasesInLibrary, $"atlas_{atlasIndex}.png");
            image.SaveAsPng(path);
            ///////////////////////////////////////////////////////////


            atlasPixels = Compression.Compress(atlasPixels);
            Asset_TextureAtlas atlasTexture = new Asset_TextureAtlas()
            {
                Pixels = atlasPixels, TextureSize = new Vector2(AtlasWidth, AtlasWidth),
                PathInLibraryFolder = atlasPath,
                DataIsCompressed = true
            };


            Serializer.SaveAssetJSON<Asset_TextureAtlas>(atlasPath, atlasTexture);

            atlasTexture.TextureAtlasMembers = _textureAtlasMembers;

            atlasIndex++;
        }


        // int atlasesCount = (int)MathF.Ceiling((float)bounds.Area / AtlasWidth);
        // Asset_TextureAtlas[] atlases = new Asset_TextureAtlas[atlasesCount];
        //
        // for (int atlasIndex = 0; atlasIndex < atlasesCount; atlasIndex++)
        // {
        //     
        // }

        GL.BindTexture(TextureTarget.Texture2DArray, 0);

        // All the rectangles in the array were assigned X and Y values. Bounds contains the width and height of the bin.
    }

    // private static Asset_TextureAtlas CreateTextureAtlas(AssetImportParameters_TextureAtlas assetImportParameters)
    // {
    //     Asset_TextureAtlas atlas = new Asset_TextureAtlas();
    // }
}