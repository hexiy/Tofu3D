public class Asset_Mesh : Asset<Asset_Mesh>
{
    public AssetHandle ModelAssetHandle; // serialize
    public float[] VertexBufferData; // serialize
    public int[] CountsOfElements; // serialize
    public int Vao;
    public int VerticesCount;
    public int VertexBufferDataLength;
}