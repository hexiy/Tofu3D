public class MeshFile : Asset<MeshFile>
{
    public Mesh Mesh;
    public byte[] ByteIndices; // serialize
    public byte[] ByteGeometryBufferData; // serialize
    public bool UsesIndices;

    public void CleanData()
    {
        ByteIndices = null;
        ByteGeometryBufferData = null;
        CanBeSerialized = false;
        GC.Collect();
    }

    public override void BeforeSerialized()
    {
        CompressData();
        base.BeforeSerialized();
    }

    public override void OnDeserialized()
    {
        DecompressData();
        base.OnDeserialized();

        if (UsesIndices != RenderingSettings.USE_INDICES)
        {
            Tofu.AssetImportManager.ImportAsset(PathInAssetsFolder, reimportIfExists: true);
            Debug.Log("Reimporting mesh file since it was serialized with different USE_INDICES state");
            // return;
        }
    }


    public void CompressData()
    {
        if (DataIsCompressed)
        {
            return;
        }

        ByteIndices = Compression.Compress(Mesh.Indices);
        ByteGeometryBufferData = Compression.Compress(Mesh.GeometryBufferData);
        DataIsCompressed = true;
    }

    public void DecompressData()
    {
        if (DataIsCompressed == false)
        {
            return;
        }

        Mesh.Indices = Compression.DecompressUnsignedIntArray(ByteIndices);
        Mesh.IndicesLength = Mesh.Indices.Length;
        Mesh.GeometryBufferData = Compression.DecompressFloatArray(ByteGeometryBufferData);

        DataIsCompressed = false;

        CleanData();
    }
}