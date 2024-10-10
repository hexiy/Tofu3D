public class Asset_Mesh : Asset<Asset_Mesh>
{
    // public RuntimeAssetHandle ModelRuntimeAssetHandle; // serialize
    public float[] VertexBufferData; // serialize
    public int[] CountsOfElements; // serialize
    public int VerticesCount; // serialize, i dont need this but its fine
}