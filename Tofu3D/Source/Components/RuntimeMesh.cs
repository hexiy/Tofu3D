public class RuntimeMesh : Asset<RuntimeMesh>
{
    public int Vao;
    public int Ebo;
    public Mesh Mesh;

    public void CleanMeshData()
    {
        Mesh.GeometryBufferData = null;
        Mesh.Indices = null;
        CanBeSerialized = false;
        GC.Collect();
    }
}