using Newtonsoft.Json;

public class Mesh:IHasPath
{
    [XmlIgnore] // for scene
    [JsonIgnore]
    public float[] VertexBufferData; // dont serialize

    public int[] CountsOfElements; // serialize
    public int VerticesCount; // serialize, i dont need this but its fine

    [XmlIgnore] // for scene
    [JsonIgnore]
    public uint[] Indices; // dont serialize

    public string Name;
//  PathToMeshFileInLibrary
    public string? Path { get; set; }
}