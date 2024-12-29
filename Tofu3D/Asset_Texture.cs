using System.Text.Json.Serialization;

namespace Tofu3D;

[Serializable]
public class Asset_Texture : Asset<Asset_Texture>
{

    public byte[] Pixels;//must be public for serialization

    public Vector2 TextureSize;

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

    public void CompressPixels()
    {
        if (DataIsCompressed)
        {
            return;
        }

        Pixels = Compression.Compress(Pixels);
        DataIsCompressed = true;
    }

    public void DecompressPixels()
    {
        if (DataIsCompressed == false)
        {
            return;
        }

        Pixels = Compression.Decompress(Pixels);
        DataIsCompressed = false;
    }
}