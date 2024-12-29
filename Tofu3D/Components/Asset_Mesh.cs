using Newtonsoft.Json;

public class Asset_Mesh : Asset<Asset_Mesh>
{
    // public RuntimeAssetHandle ModelRuntimeAssetHandle; // serialize
    [JsonIgnore]
    public float[] VertexBufferData;// dont serialize
    public int[] CountsOfElements; // serialize
    public int VerticesCount; // serialize, i dont need this but its fine

    [JsonIgnore]
    public uint[] Indices; // dont serialize
    public byte[] ByteIndices; // serialize
    public byte[] ByteVertexBufferData; // serialize

    public override void BeforeSerialized()
    {
        CompressData();
        base.BeforeSerialized();
    }

    public override void OnDeserialized()
    {
        DecompressData();
        base.OnDeserialized();
    }


    public void CompressData()
    {
        if (DataIsCompressed)
        {
            return;
        }

        ByteIndices = Compression.Compress(Indices);
        ByteVertexBufferData = Compression.Compress(VertexBufferData);
        DataIsCompressed = true;
    }

    public void DecompressData()
    {
        if (DataIsCompressed == false)
        {
            return;
        }

        Indices = Compression.DecompressUnsignedIntArray(ByteIndices);
        VertexBufferData = Compression.DecompressFloatArray(ByteVertexBufferData);
        
        DataIsCompressed = false;
    }
}