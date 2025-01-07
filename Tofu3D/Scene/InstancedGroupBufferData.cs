namespace Tofu3D.Rendering.Instancing;


/// <summary>
/// Contains data of multiple(instanced together) objects in one buffer
/// </summary>
public class InstancedGroupBufferData
{
    public int
        InstancedVertexDataSizeInBytes;


    public float[] Buffer;

    // public bool IsResizing = false;
    public List<int> EmptyStartIndexes;
    public int FutureMaxNumberOfObjects;
    public int MaxNumberOfObjects;

    public bool NeedsUpload = true;
    public int NumberOfObjects;
    public int Vbo;
    public int Vao;
    public int Ebo;
    public int ShaderId;
    public bool UVOffsetIsInstanced = false;
    public RenderMode RenderMode;

    public required VertexBufferStructureType VertexBufferStructureType { init; get; }

    public int InstancedVertexCountOfFloats => InstancedVertexDataSizeInBytes / sizeof(float);

    public void Init()
    {
        InstancedVertexDataSizeInBytes =
            // sizeof(float) * 3 * 4 + sizeof(float) * 4; // 4x vec 3's for matrix+vec4 color;
            sizeof(float) * 3 * 4 // 4x vec 3's for model_1 model_2 model_3 model_4
            + (UVOffsetIsInstanced
                ? (sizeof(float) * 2) // 1x vector2 for uv offset
                : 0) + (sizeof(float)); // 1 int for id-but doing float for now...
    }
}