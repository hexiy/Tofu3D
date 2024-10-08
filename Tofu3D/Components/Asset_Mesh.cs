public class Mesh : Asset<Mesh>
{
    public AssetHandle ModelAssetHandle; // serialize
    public float[] VertexBufferData; // serialize
    public int[] CountsOfElements; // serialize
}