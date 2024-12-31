using Newtonsoft.Json;

public class MeshFile : Asset<MeshFile>
{
    public Mesh Mesh;
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

        ByteIndices = Compression.Compress(Mesh.Indices);
        ByteVertexBufferData = Compression.Compress(Mesh.VertexBufferData);
        DataIsCompressed = true;
    }

    public void DecompressData()
    {
        if (DataIsCompressed == false)
        {
            return;
        }

        Mesh.Indices = Compression.DecompressUnsignedIntArray(ByteIndices);
        Mesh.VertexBufferData = Compression.DecompressFloatArray(ByteVertexBufferData);

        DataIsCompressed = false;
    }
}