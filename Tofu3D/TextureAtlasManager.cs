using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using LibNoise.Renderer;
using OpenTK.Mathematics;
using RectpackSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace Tofu3D;

public class TextureAtlasManager
{
    private int AtlasWidth = -1;
    public int GLTextureArrayId;
    private int _atlasesCount;


    public void SetupTextureAtlases()
    {
        List<Asset_Texture> textures = new List<Asset_Texture>();

        List<string> paths = new List<string>();
        paths.AddRange(Directory.GetFiles(Folders.TexturesInLibrary, "*.tofutexture", SearchOption.TopDirectoryOnly));
        foreach (string rawAssetPath in paths)
        {
            Asset_Texture assetTexture = Serializer.ReadAssetJSON<Asset_Texture>(rawAssetPath);
            textures.Add(assetTexture);
        }

        GenerateAtlasesForTextures(textures);
    }

    private void GenerateAtlasesForTextures(List<Asset_Texture> textures)
    {
        if (AtlasWidth == -1)
        {
            AtlasWidth = GL.GetInteger(GetPName.MaxTextureSize);
            // AtlasWidth = (int)Mathf.ClampMax(AtlasWidth, 16_384);
            AtlasWidth = (int)Mathf.ClampMax(AtlasWidth, 8192);
            // AtlasWidth = (int)Mathf.ClampMax(AtlasWidth, 4092); //8192);
        }

        textures.Sort();

        // pack textures into atlases
        List<PackingRectangle[]> atlasesRectangles = PackTexturesIntoAtlases(textures);

        // create the texture array
        _atlasesCount = atlasesRectangles.Count;
        SetupTextureArray();

        // populate atlases with textures
        GenerateAndSaveAtlases(atlasesRectangles, textures);

        GL.BindTexture(TextureTarget.Texture2DArray, 0);
    }

    private List<PackingRectangle[]> PackTexturesIntoAtlases(List<Asset_Texture> textures)
    {
        List<PackingRectangle> currentAtlasRectangles = new List<PackingRectangle>();

        List<PackingRectangle[]> atlasesRectangles = new List<PackingRectangle[]>();

        for (int i = 0; i < textures.Count; i++)
        {
            PackingRectangle rectangleToBePacked = new PackingRectangle(
                x: 0,
                y: 0,
                width: (uint)textures[i].TextureSize.X,
                height: (uint)textures[i].TextureSize.Y,
                id: i
            );
            currentAtlasRectangles.Add(rectangleToBePacked);

            PackingRectangle[] rectanglesToBePacked = currentAtlasRectangles.ToArray();
            bool packed = TryPackTextures(rectanglesToBePacked, out PackingRectangle bounds);
            if (packed == false)
            {
                currentAtlasRectangles.Remove(rectangleToBePacked);
            }

            if (packed)
            {
                currentAtlasRectangles = rectanglesToBePacked.ToList();
            }

            bool currentAtlasIsDone = packed == false || i == textures.Count - 1;
            if (currentAtlasIsDone)
            {
                atlasesRectangles.Add(currentAtlasRectangles.ToArray());

                currentAtlasRectangles.Clear();
                if (packed == false)
                {
                    i--; // pack current texture next iteration because it failed
                }
            }
        }

        return atlasesRectangles;
    }

    private bool TryPackTextures(PackingRectangle[] rects, out PackingRectangle bounds)
    {
        try
        {
            RectanglePacker.Pack(
                rects,
                out bounds,
                maxBoundsHeight: (uint)AtlasWidth,
                maxBoundsWidth: (uint)AtlasWidth
            );
            return true;
        }
        catch
        {
            bounds = default;
            return false;
        }
    }

    private void GenerateAndSaveAtlases(List<PackingRectangle[]> atlasesRectangles, List<Asset_Texture> textures)
    {
        int atlasIndex = 0;

        foreach (PackingRectangle[] textureRectangles in atlasesRectangles)
        {
            byte[] atlasPixels =
                GenerateAtlasPixels(textureRectangles, textures, out TextureAtlasMember[] atlasMembers, atlasIndex);

            UploadAtlasToTextureArray(atlasPixels, atlasIndex);

            if (File.Exists(GetAtlasPath(atlasIndex)) == false) // temporary only
            {
                SaveAtlasAsset(atlasPixels, atlasMembers, atlasIndex);
            }

            atlasIndex++;
        }
    }

    private byte[] _atlasPixels;

    private byte[] GenerateAtlasPixels(PackingRectangle[] textureRectangles, List<Asset_Texture> textures,
        out TextureAtlasMember[] atlasMembers, int atlasIndex)
    {
        atlasMembers = new TextureAtlasMember[textureRectangles.Length];

        if (_atlasPixels == null)
        {
            _atlasPixels = new byte[4 * AtlasWidth * AtlasWidth];
        }
        else
        {
            Array.Clear(_atlasPixels, 0, _atlasPixels.Length);
        }

        for (int i = 0; i < textureRectangles.Length; i++)
        {
            PackingRectangle rectangle = textureRectangles[i];
            Asset_Texture texture = textures[rectangle.Id];
            texture.OnDeserialized();

            FillAtlasPixels(ref _atlasPixels, rectangle, texture.Pixels);

            Vector4 box = new Vector4(
                rectangle.X,
                rectangle.Y,
                rectangle.Right,
                rectangle.Bottom);


            box = box / (float)AtlasWidth; // 0 - 1

            atlasMembers[i] = new TextureAtlasMember()
            {
                BoundingBox = box,
                PathToTextureInLibrary = texture.PathInAssetsFolder,
            };

            texture.BoundingBoxInAtlas = box;
            texture.AtlasPath = GetAtlasPath(atlasIndex);
            texture.IndexInAtlasTextureArray = atlasIndex;

            Tofu.AssetLoadManager.Save(texture.PathInLibraryFolder, texture);
        }

        return _atlasPixels;
    }

    private void FillAtlasPixels(ref byte[] atlasPixels, PackingRectangle rectangle, byte[] texturePixels)
    {
        int textureWidth = (int)rectangle.Width;
        int textureHeight = (int)rectangle.Height;

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int atlasX = (int)rectangle.X + x;
                int atlasY = (int)rectangle.Y + (textureHeight - 1 - y); // Flip vertically

                int indexInAtlasPixels = (atlasY * AtlasWidth + atlasX) * 4;
                int indexInTexturePixels = (y * textureWidth + x) * 4;

                atlasPixels[indexInAtlasPixels] = texturePixels[indexInTexturePixels]; // R
                atlasPixels[indexInAtlasPixels + 1] = texturePixels[indexInTexturePixels + 1]; // G
                atlasPixels[indexInAtlasPixels + 2] = texturePixels[indexInTexturePixels + 2]; // B
                atlasPixels[indexInAtlasPixels + 3] = texturePixels[indexInTexturePixels + 3]; // A
            }
        }
    }


    private void SaveAtlasAsset(byte[] atlasPixels, TextureAtlasMember[] atlasMembers, int atlasIndex)
    {
        ///////////////////////////////////////////////////////////// PNG
        // if (false)
        {
            using Image<Rgba32>? image =
                Image.LoadPixelData<Rgba32>(atlasPixels, AtlasWidth, AtlasWidth);
            image.Mutate(x => { x.Flip(FlipMode.Vertical); });
            // byte[] pixels = new byte[AtlasWidth * AtlasWidth * 4];
            // image.CopyPixelDataTo(pixels);


            string path = Path.Combine(Folders.TextureAtlasesInLibrary, $"atlas_{atlasIndex}.png");
            image.SaveAsPng(path);
        }
        /////////////////////////////////////////////////////////////
        string textFile = "";
        for (int i = 0; i < atlasMembers.Length; i++)
        {
            textFile +=
                $"{Path.GetFileName(atlasMembers[i].PathToTextureInLibrary)}: {atlasMembers[i].BoundingBox}{Environment.NewLine}";
        }

        string textFilePath = Path.Combine(Folders.TextureAtlasesInLibrary, $"atlas_{atlasIndex}.txt");

        // File.WriteAllText(textFilePath, textFile, new UTF8Encoding(false));  // UTF-8 without BOM

        Serializer.SaveTextFile(path: textFilePath, text: textFile);


        string atlasPath = GetAtlasPath(atlasIndex);

        atlasPixels = Compression.Compress(atlasPixels);

        Asset_TextureAtlas atlasTexture = new Asset_TextureAtlas()
        {
            Pixels = atlasPixels,
            TextureSize = new Vector2(AtlasWidth, AtlasWidth),
            PathInLibraryFolder = atlasPath,
            DataIsCompressed = true,
            TextureAtlasMembers = atlasMembers,
            IndexInTextureArray = atlasIndex,
        };

        Serializer.SaveAssetJSON<Tofu3D.Asset_TextureAtlas>(atlasPath, atlasTexture);
    }

    private string GetAtlasPath(int atlasIndex)
    {
        string atlasPath = Path.Combine(Folders.TextureAtlasesInLibrary, $"atlas_{atlasIndex}.tofutextureatlas");
        return atlasPath;
    }

    private void UploadAtlasToTextureArray(byte[] atlasPixels, int atlasIndex)
    {
        GL.TexSubImage3D(
            TextureTarget.Texture2DArray,
            0,
            0, 0, atlasIndex,
            AtlasWidth,
            AtlasWidth,
            1,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            atlasPixels
        );
    }

    private void SetupTextureArray()
    {
        GLTextureArrayId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DArray, GLTextureArrayId);

        GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba, AtlasWidth, AtlasWidth,
            _atlasesCount, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

        float[] borderColor =
        {
            1.0f, 0.0f, 1.0f, 1.0f
        };
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureBorderColor, borderColor);

        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToBorder);
    }

    /*private void GenerateAtlasesForTextures(List<Asset_Texture> textures)
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
                box = box / (float)AtlasWidth;
                if (MathF.Abs(box.X) > 1 ||
                    MathF.Abs(box.Y) > 1 ||
                    MathF.Abs(box.Z) > 1 ||
                    MathF.Abs(box.W) > 1)
                {
                    Debug.LogError("wrong bounds");
                }


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
            if (false)
            {
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
            }
            ///////////////////////////////////////////////////////////


            atlasPixels = Compression.Compress(atlasPixels);
            Asset_TextureAtlas atlasTexture = new Asset_TextureAtlas()
            {
                Pixels = atlasPixels, TextureSize = new Vector2(AtlasWidth, AtlasWidth),
                PathInLibraryFolder = atlasPath,
                DataIsCompressed = true
            };

            atlasTexture.TextureAtlasMembers = _textureAtlasMembers;

            Serializer.SaveAssetJSON<Asset_TextureAtlas>(atlasPath, atlasTexture);


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
*/
}