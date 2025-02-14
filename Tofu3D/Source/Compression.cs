using System.IO;
using System.IO.Compression;
using Buffer = System.Buffer;

namespace TofuEngine;

public static class Compression
{
    public static byte[] Compress(byte[] data)
    {
        if (data == null)
        {
            return data;
        }

        using (MemoryStream memoryStream = new MemoryStream())
        using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal))
        {
            deflateStream.Write(data, 0, data.Length);
            deflateStream.Close(); // Ensure all data is flushed
            return memoryStream.ToArray(); // Get the compressed bytes
        }
    }

    public static byte[] Decompress(byte[] compressedData)
    {
        if (compressedData == null)
        {
            return compressedData;
        }

        using (MemoryStream compressedStream = new MemoryStream(compressedData))
        using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
        using (MemoryStream resultStream = new MemoryStream())
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

    // Convert float[] to byte[] for Deflate processing
    public static byte[] ConvertFloatArrayToByteArray(float[] floatArray)
    {
        byte[] byteArray = new byte[floatArray.Length * sizeof(float)];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    // Convert byte[] back to float[] after decompression
    public static float[] ConvertByteArrayToFloatArray(byte[] byteArray)
    {
        float[] floatArray = new float[byteArray.Length / sizeof(float)];
        Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
        return floatArray;
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

    // Compress float[] array
    public static byte[] Compress(float[] data)
    {
        byte[] byteArray = ConvertFloatArrayToByteArray(data); // Convert to byte array
        return Compress(byteArray); // Compress the byte array
    }

    // Decompress uint[] array
    public static uint[] DecompressUnsignedIntArray(byte[] compressedData)
    {
        byte[] decompressedByteArray = Decompress(compressedData); // Decompress compressed data
        return ConvertByteArrayToUIntArray(decompressedByteArray); // Convert back to uint array
    }

    // Decompress uint[] array
    public static float[] DecompressFloatArray(byte[] compressedData)
    {
        byte[] decompressedByteArray = Decompress(compressedData); // Decompress compressed data
        return ConvertByteArrayToFloatArray(decompressedByteArray); // Convert back to uint array
    }
}