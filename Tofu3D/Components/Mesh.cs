using Newtonsoft.Json;

public class Mesh
{
    [JsonIgnore]
    public float[] VertexBufferData; // dont serialize

    public int[] CountsOfElements; // serialize
    public int VerticesCount; // serialize, i dont need this but its fine

    [JsonIgnore]
    public uint[] Indices; // dont serialize

    public string Name;
    public string? PathToMeshFileInLibrary=null;
}