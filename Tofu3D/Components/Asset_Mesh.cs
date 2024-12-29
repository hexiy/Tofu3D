public class Asset_Mesh : Asset<Asset_Mesh>
{
    // public RuntimeAssetHandle ModelRuntimeAssetHandle; // serialize
    public float[] VertexBufferData; // serialize
    public int[] CountsOfElements; // serialize
    public int VerticesCount; // serialize, i dont need this but its fine
    private uint[] _indices; // dont serialize
    public byte[] ByteIndices; // serialize

    public void SetIndices(uint[] indices)
    {
        _indices = indices;
    }

    public uint[] GetIndices()
    {
        return _indices;
    }
    public override void BeforeSerialized()
    {
        CompressIndices();
        base.BeforeSerialized();
    }

    public override void OnDeserialized()
    {
        DecompressIndices();
        base.OnDeserialized();
    }

    public void CompressIndices()
    {
        if (DataIsCompressed)
        {
            return;
        }

        ByteIndices = Compression.Compress(_indices);
        DataIsCompressed = true;
    }

    public void DecompressIndices()
    {
        if (DataIsCompressed == false)
        {
            return;
        }

        _indices = Compression.DecompressUnsignedIntArray(ByteIndices);
        DataIsCompressed = false;
    }
}