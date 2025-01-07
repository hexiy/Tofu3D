namespace Tofu3D.Rendering.Instancing;

/// <summary>
/// Contains data of multiple(instanced together) objects in one buffer
/// </summary>
public class InstancedGroupBufferData
{
    public int
        InstancedVertexDataSizeInBytes;


    public float[] Buffer;
    public List<ObjectInstancingData> ObjectInstancingDatas = new List<ObjectInstancingData>();

    // public bool IsResizing = false;
    private List<int> EmptyStartIndexes = new List<int>();
    public int FutureMaxNumberOfObjects;
    public int MaxNumberOfObjects;

    public bool NeedsUpload = true;
    public int NumberOfObjects { get; private set; } = 0;
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

    public void AddObject(ObjectInstancingData objectInstancingData)
    {
        ObjectInstancingDatas.Add(objectInstancingData);
        NumberOfObjects++;
    }

    public void RemoveObject(ObjectInstancingData removedObjectInstancingData)
    {
        for (var i = 0; i < Buffer.Length - InstancedVertexCountOfFloats; i++)
        {
            Buffer[i] = Buffer[i + InstancedVertexCountOfFloats];
        }

        // go through all objectInstancingData that is in this buffer and change their starting index if they are after this one
        int indexOfThis =
            ObjectInstancingDatas.FindIndex(o => o.Guid == removedObjectInstancingData.Guid);


        ObjectInstancingDatas.RemoveAt(indexOfThis);


        // we dont need to update EmptyStartIndexes if we just shift the whole buffer down to fill the newly emptied space
        // EmptyStartIndexes.Add(removedObjectInstancingData.StartingIndexInBuffer);
        NumberOfObjects--;

        for (int i = indexOfThis; i < ObjectInstancingDatas.Count; i++)
        {
            var oid = ObjectInstancingDatas[i];
            oid.StartingIndexInBuffer =
                oid.StartingIndexInBuffer - InstancedVertexCountOfFloats;
            ObjectInstancingDatas[i] = oid;
        }
    }

    public int GetEmptyIndex()
    {
        if (EmptyStartIndexes.Count > 0)
        {
            var index = EmptyStartIndexes[0];
            EmptyStartIndexes.RemoveAt(0);
            return index;
        }

        if (NumberOfObjects == MaxNumberOfObjects)
        {
            FutureMaxNumberOfObjects += 1;
            return -1;
        }

        if (Buffer.Length <
            InstancedVertexCountOfFloats * MaxNumberOfObjects)
        {
            return -1;
        }

        return NumberOfObjects * InstancedVertexCountOfFloats;
    }
}