
public class RuntimeMesh : Asset<RuntimeMesh>
{
    public string MeshAssetPath;
    public int Vao;
    public int Ebo;
    public int VerticesCount;
    public int VertexBufferDataLength;
    public int IsDynamic;
    public uint[] Indices;
}