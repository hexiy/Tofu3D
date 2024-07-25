using System.Text.Json;

namespace Tofu3D;

public class InstancedRenderingObjectBufferData
{
    public float[] Buffer;
    public int NumberOfObjects;
    public int MaxNumberOfObjects;
    public int FutureMaxNumberOfObjects;
    public int Vbo;

    public bool NeedsUpload = true;

    // public bool IsResizing = false;
    public List<int> EmptyStartIndexes;


    private VertexBufferStructureType _vertexBufferStructureType;
    public required VertexBufferStructureType VertexBufferStructureType
    {
        init
        {
            _vertexBufferStructureType = value;
        }
        get => _vertexBufferStructureType;
    }

    public readonly int
        _instancedVertexDataSizeInBytes = sizeof(float) * 4 * 3 + sizeof(float) * 4; // 4x vec 3's for matrix+vec4 color;

    public int InstancedVertexCountOfFloats => _instancedVertexDataSizeInBytes / sizeof(float);
    
    
}