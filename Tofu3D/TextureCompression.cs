using System.IO;
using System.IO.Compression;

namespace Tofu3D;

public static class TextureCompression
{
    public static byte[] CompressWithDeflate(byte[] data)
    {
        using (var memoryStream = new MemoryStream())
        using (var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal))
        {
            deflateStream.Write(data, 0, data.Length);
            deflateStream.Close(); // Ensure all data is flushed
            return memoryStream.ToArray(); // Get the compressed bytes
        }
    }
    public static byte[] DecompressWithDeflate(byte[] compressedData)
    {
        using (var compressedStream = new MemoryStream(compressedData))
        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            deflateStream.CopyTo(resultStream); // Copy decompressed data into result stream
            return resultStream.ToArray(); // Return the decompressed bytes
        }
    }
}