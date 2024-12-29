using System.Text.Json.Serialization;

namespace Tofu3D;

[Serializable]
public class Asset_Texture : Asset<Asset_Texture>
{
    public bool PixelsAreCompressed = false;

    public byte[] _pixels;//must be public for serialization

    public Vector2 TextureSize;

    public byte[] GetPixels()
    {
        if (_pixels == null)
        {
            return null;
        }

        DecompressPixels();
        return _pixels;
    }

    public void SetPixels(byte[] pixels)
    {
        _pixels = pixels;
        CompressPixels();
    }

    public void CompressPixels()
    {
        if (PixelsAreCompressed)
        {
            return;
        }

        _pixels = TextureCompression.CompressWithDeflate(_pixels);
        PixelsAreCompressed = true;
    }

    public void DecompressPixels()
    {
        if (PixelsAreCompressed == false)
        {
            return;
        }

        _pixels = TextureCompression.DecompressWithDeflate(_pixels);
        PixelsAreCompressed = false;
    }
}