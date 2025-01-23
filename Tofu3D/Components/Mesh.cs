using Newtonsoft.Json;

public class Mesh : IHasPath
{
    [XmlIgnore] // for scene xml serialization
    [JsonIgnore]
    public float[] GeometryBufferData; // dont serialize

    public int[] CountsOfElements; // serialize
    public int VerticesCount; // serialize, i dont need this but its fine

    [XmlIgnore] // for scene xml serialization
    [JsonIgnore]
    public uint[] Indices; // dont serialize

    public string Name;

    public string? PathInAssetsFolder { get; set; }

    // MeshFile path
    public string? PathInLibraryFolder { get; set; }

    // this should be in MeshFile but I'll have it here for now
    public string? PathToObjMaterial;
}