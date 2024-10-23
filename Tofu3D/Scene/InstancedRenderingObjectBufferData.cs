namespace Tofu3D;

public class InstancedRenderingObjectBufferData
{
    public readonly int
        _instancedVertexDataSizeInBytes =
            // sizeof(float) * 3 * 4 + sizeof(float) * 4; // 4x vec 3's for matrix+vec4 color;
            sizeof(float) * 3 * 4; // 4x vec 3's for model_1 model_2 model_3 model_4


    public float[] Buffer;

    // public bool IsResizing = false;
    public List<int> EmptyStartIndexes;
    public int FutureMaxNumberOfObjects;
    public int MaxNumberOfObjects;

    public bool NeedsUpload = true;
    public int NumberOfObjects;
    public int Vbo;

    public required VertexBufferStructureType VertexBufferStructureType { init; get; }

    public int InstancedVertexCountOfFloats => _instancedVertexDataSizeInBytes / sizeof(float);
}