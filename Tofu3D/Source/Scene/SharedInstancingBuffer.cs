namespace TofuEngine.Rendering.Instancing;

/// <summary>
/// Contains data of multiple(instanced together) objects in one buffer
/// </summary>
public class SharedInstancingBuffer
{
    public float[] InstancingBuffer;
    private List<ObjectInstancingData> ObjectInstancingDatas = new List<ObjectInstancingData>();

    // public bool IsResizing = false;
    private List<int> EmptyStartIndexes = new List<int>();
    public int MaxNumberOfObjects;

    public bool NeedsUpload = true;
    public int NumberOfObjects { get; private set; } = 0;
    public int Vbo;
    public int Vao;
    public int Ebo;
    public int ShaderId;
    public bool UVOffsetIsInstanced = false;
    public RenderMode RenderMode;

    // public required VertexBufferStructureType VertexBufferStructureType { init; get; }
    public InstancedGroupDefinition InstancedGroupDefinition;

    // public SharedInstancingBuffer(Shader shader)
    // {
    // }
    

    public SharedInstancingBuffer(InstancedGroupDefinition instancedGroupDefinition)
    {
        // Debug.Log("Initializing Instanced Buffer Data");
        Tofu.ShaderManager.BindVertexArray(instancedGroupDefinition.RuntimeMesh.Vao);

        instancedGroupDefinition.Material.LoadShader();
        if (instancedGroupDefinition.Material.Shader.IsLoaded == false)
        {
            Debug.LogError("Couldnt load shader");
            throw new Exception("Couldnt load shader");
        }

        InstancedGroupDefinition = instancedGroupDefinition;
        // VertexBufferStructureType = instancedGroupDefinition.vertexBufferStructureType;
        MaxNumberOfObjects = 1;
        // FutureMaxNumberOfObjects = 1;
        Vbo = -1;
        Vao = instancedGroupDefinition.RuntimeMesh.Vao;
        // Ebo = objectDefinition.RuntimeMesh.Ebo;
        ShaderId = instancedGroupDefinition.Material.Shader.ProgramId;
        UVOffsetIsInstanced = instancedGroupDefinition.Material.UVOffsetIsInstanced;
        RenderMode = instancedGroupDefinition.Material.RenderMode;

        InstancingBuffer = new float[MaxNumberOfObjects *
                                     InstancedVertexDataLayoutDefinition.CountOfFloats];

        SetupInstancedBufferAndUploadIfNeeded();
    }

    public void AddObject(ref ObjectInstancingData objectInstancingData)
    {
        objectInstancingData.StartingIndexInBuffer = GetEmptyIndex();
        ObjectInstancingDatas.Add(objectInstancingData);
        NumberOfObjects++;
    }

    public void ExpandBuffer()
    {
        this.MaxNumberOfObjects += 5;
        if (this.MaxNumberOfObjects > 50)
        {
            this.MaxNumberOfObjects += 20;
        }

        // Debug.Log($"Resizing buffer to new size:{this.MaxNumberOfObjects}");

        Array.Resize(ref this.InstancingBuffer,
            this.MaxNumberOfObjects * InstancedVertexDataLayoutDefinition.CountOfFloats);
        GL.DeleteBuffer(this.Vbo);
        this.Vbo = -1;
        this.NeedsUpload = true;
    }

    public void RemoveObject(ObjectInstancingData removedObjectInstancingData)
    {
        for (int i = removedObjectInstancingData.StartingIndexInBuffer;
             i < InstancingBuffer.Length - InstancedVertexDataLayoutDefinition.CountOfFloats;
             i++)
        {
            InstancingBuffer[i] = InstancingBuffer[i + InstancedVertexDataLayoutDefinition.CountOfFloats];
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
            ObjectInstancingData oid = ObjectInstancingDatas[i];
            oid.StartingIndexInBuffer =
                oid.StartingIndexInBuffer - InstancedVertexDataLayoutDefinition.CountOfFloats;
            ObjectInstancingDatas[i] = oid;
        }
    }

    private int GetEmptyIndex()
    {
        if (EmptyStartIndexes.Count > 0)
        {
            int index = EmptyStartIndexes[0];
            EmptyStartIndexes.RemoveAt(0);
            return index;
        }

        if (NumberOfObjects == MaxNumberOfObjects)
        {
            ExpandBuffer();

            // FutureMaxNumberOfObjects += 1;
            // return -1;
        }

        if (InstancingBuffer.Length <
            InstancedVertexDataLayoutDefinition.CountOfFloats * MaxNumberOfObjects)
        {
            ExpandBuffer();
            // return -1;
        }

        return NumberOfObjects * InstancedVertexDataLayoutDefinition.CountOfFloats;
    }

    public void SetupInstancedBufferAndUploadIfNeeded()
    {
        Tofu.ShaderManager.BindVertexArray(this.Vao);


        bool newBuffer = this.Vbo == -1;
        if (newBuffer)
        {
            NeedsUpload = true;
        }

        if (newBuffer)
        {
            this.Vbo = GL.GenBuffer();
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, this.Vbo);
        if (NeedsUpload)
        {
            if (newBuffer)
            {
                // this should be called only once but it simply doesnt work... i need to call GL.VertexAttribPointer every frame
                // https://stackoverflow.com/a/28597384
                int bytesOffset = 0;
                int vertexAttribPointerIndex = 5;

                for (int i = 0; i < InstancedVertexDataLayoutDefinition.Members.Count; i++)
                {
                    int numberOfFloatsInAttribute = InstancedVertexDataLayoutDefinition.Members[i];
                    GL.VertexAttribPointer(vertexAttribPointerIndex++, numberOfFloatsInAttribute,
                        VertexAttribPointerType.Float, false,
                        InstancedVertexDataLayoutDefinition.TotalSizeOfVertexInBytes,
                        bytesOffset);
                    bytesOffset += numberOfFloatsInAttribute * sizeof(float);
                }
            }

            if (newBuffer)
            {
                // unique attribs for each instance
                int vertexAttribArrayIndex = 5;
                int vertexAttribDivisorIndex = 5;

                for (int i = 0; i < InstancedVertexDataLayoutDefinition.Members.Count; i++)
                {
                    GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
                    GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
                }
            }


            GL.BufferData(BufferTarget.ArrayBuffer,
                sizeof(float) * this.InstancingBuffer.Length,
                this.InstancingBuffer, BufferUsageHint.StaticDraw);

            this.NeedsUpload = false;
        }
    }
}