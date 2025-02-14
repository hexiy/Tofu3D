using OpenTK.Mathematics;

namespace TofuEngine;

[Serializable]
public class Asset_Texture : Asset<Asset_Texture>, IComparable<Asset_Texture>
{
    public byte[] Pixels; //must be public for serialization

    public Vector2 TextureSize;
    public string AtlasPath;
    public Vector4 BoundingBoxInAtlas;
    public int IndexInAtlasTextureArray;

    public override void BeforeSerialized()
    {
        CompressPixels();
        base.BeforeSerialized();
    }

    public override void OnDeserialized()
    {
        DecompressPixels();
        base.OnDeserialized();
    }

    private void CompressPixels()
    {
        if (DataIsCompressed)
        {
            return;
        }

        Pixels = Compression.Compress(Pixels);
        DataIsCompressed = true;
    }

    private void DecompressPixels()
    {
        if (DataIsCompressed == false)
        {
            return;
        }

        Pixels = Compression.Decompress(Pixels);
        DataIsCompressed = false;
    }

    public int CompareTo(Asset_Texture? other)
    {
        if (this.TextureSize.X * this.TextureSize.Y > other.TextureSize.X * other.TextureSize.Y)
        {
            return 1;
        }

        if (this.TextureSize.X * this.TextureSize.Y == other.TextureSize.X * other.TextureSize.Y)
        {
            return 0;
        }

        if (this.TextureSize.X * this.TextureSize.Y < other.TextureSize.X * other.TextureSize.Y)
        {
            return -1;
        }

        return 0;
    }
    
    public void CleanPixelData()
    {
        Pixels = null;
        CanBeSerialized = false;
        GC.Collect();
    }
}