using System.IO;
using System.IO.Compression;
using Buffer = System.Buffer;

namespace Tofu3D;

public static class Compression
{
    public static byte[] Compress(byte[] data)
    {
        using (var memoryStream = new MemoryStream())
        using (var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal))
        {
            deflateStream.Write(data, 0, data.Length);
            deflateStream.Close(); // Ensure all data is flushed
            return memoryStream.ToArray(); // Get the compressed bytes
        }
    }

    public static byte[] Decompress(byte[] compressedData)
    {
        using (var compressedStream = new MemoryStream(compressedData))
        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            deflateStream.CopyTo(resultStream); // Copy decompressed data into result stream
            return resultStream.ToArray(); // Return the decompressed bytes
        }
    }

    // Convert uint[] to byte[] for Deflate processing
    public static byte[] ConvertUIntArrayToByteArray(uint[] uintArray)
    {
        byte[] byteArray = new byte[uintArray.Length * sizeof(uint)];
        Buffer.BlockCopy(uintArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    // Convert byte[] back to uint[] after decompression
    public static uint[] ConvertByteArrayToUIntArray(byte[] byteArray)
    {
        uint[] uintArray = new uint[byteArray.Length / sizeof(uint)];
        Buffer.BlockCopy(byteArray, 0, uintArray, 0, byteArray.Length);
        return uintArray;
    }

    // Compress uint[] array
    public static byte[] Compress(uint[] data)
    {
        byte[] byteArray = ConvertUIntArrayToByteArray(data); // Convert to byte array
        return Compress(byteArray); // Compress the byte array
    }

    // Decompress uint[] array
    public static uint[] DecompressUnsignedIntArray(byte[] compressedData)
    {
        byte[] decompressedByteArray = Decompress(compressedData); // Decompress compressed data
        return ConvertByteArrayToUIntArray(decompressedByteArray); // Convert back to uint array
    }
}