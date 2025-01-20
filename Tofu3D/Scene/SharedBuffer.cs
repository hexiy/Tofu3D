namespace Tofu3D.Rendering.Instancing;

/// <summary>
/// Contains data of multiple(instanced together) objects in one buffer
/// </summary>
public class SharedBuffer
{
    private int InstancedVertexDataSizeInBytes;

    public float[] Buffer;
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

    public required VertexBufferStructureType VertexBufferStructureType { init; get; }

    public int InstancedVertexCountOfFloats => InstancedVertexDataSizeInBytes / sizeof(float);

    public void Init()
    {
        InstancedVertexDataSizeInBytes =
            sizeof(float) * 3 * 4 // 4x vec 3's for model_1 model_2 model_3 model_4
            + (UVOffsetIsInstanced ? (sizeof(float) * 2) : 0) // 1x vector2 for uv offset
            + (sizeof(float)) // 1 int for mouse picking id
            + (sizeof(float) * 4); // 4 floats for albedo texture bounding box in atlas
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
        if (this.MaxNumberOfObjects > 1000)
        {
            this.MaxNumberOfObjects += 20;
        }

        // Debug.Log($"Resizing buffer to new size:{this.MaxNumberOfObjects}");

        Array.Resize(ref this.Buffer,
            this.MaxNumberOfObjects * this.InstancedVertexCountOfFloats);
        GL.DeleteBuffer(this.Vbo);
        this.Vbo = -1;
        this.NeedsUpload = true;
    }

    public void RemoveObject(ObjectInstancingData removedObjectInstancingData)
    {
        for (var i = removedObjectInstancingData.StartingIndexInBuffer;
             i < Buffer.Length - InstancedVertexCountOfFloats;
             i++)
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

    private int GetEmptyIndex()
    {
        if (EmptyStartIndexes.Count > 0)
        {
            var index = EmptyStartIndexes[0];
            EmptyStartIndexes.RemoveAt(0);
            return index;
        }

        if (NumberOfObjects == MaxNumberOfObjects)
        {
            ExpandBuffer();

            // FutureMaxNumberOfObjects += 1;
            // return -1;
        }

        if (Buffer.Length <
            InstancedVertexCountOfFloats * MaxNumberOfObjects)
        {
            ExpandBuffer();
            // return -1;
        }

        return NumberOfObjects * InstancedVertexCountOfFloats;
    }

    public void SetupBufferAndUploadIfNeeded()
    {
        Tofu.ShaderManager.BindVertexArray(this.Vao);


        var newBuffer = this.Vbo == -1;
        if (newBuffer)
        {
            this.Vbo = GL.GenBuffer();
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, this.Vbo);

        {
            // this should be called only once but it simply doesnt work... i need to call GL.VertexAttribPointer every frame
            // https://stackoverflow.com/a/28597384
            int offset = 0;
            int vertexAttribPointerIndex = 5;
            GL.VertexAttribPointer(vertexAttribPointerIndex++, 3, VertexAttribPointerType.Float, false,
                this.InstancedVertexDataSizeInBytes,
                offset);
            offset += 3 * sizeof(float);
            GL.VertexAttribPointer(vertexAttribPointerIndex++, 3, VertexAttribPointerType.Float, false,
                this.InstancedVertexDataSizeInBytes,
                offset);
            offset += 3 * sizeof(float);

            GL.VertexAttribPointer(vertexAttribPointerIndex++, 3, VertexAttribPointerType.Float, false,
                this.InstancedVertexDataSizeInBytes,
                offset);
            offset += 3 * sizeof(float);

            GL.VertexAttribPointer(vertexAttribPointerIndex++, 3, VertexAttribPointerType.Float, false,
                this.InstancedVertexDataSizeInBytes,
                offset);
            offset += 3 * sizeof(float);

            GL.VertexAttribPointer(vertexAttribPointerIndex++, 1, VertexAttribPointerType.Float, false,
                this.InstancedVertexDataSizeInBytes,
                offset);
            offset += 1 * sizeof(float);

            if (this.UVOffsetIsInstanced)
            {
                GL.VertexAttribPointer(vertexAttribPointerIndex++, 2, VertexAttribPointerType.Float, false,
                    this.InstancedVertexDataSizeInBytes,
                    offset);
                offset += 2 * sizeof(float);
            }


            // albedo bounding box in atlas
            GL.VertexAttribPointer(vertexAttribPointerIndex++, 4, VertexAttribPointerType.Float, false,
                this.InstancedVertexDataSizeInBytes,
                offset);
            offset += 4 * sizeof(float);
        }

        if (this.NeedsUpload && newBuffer)
        {
            // unique attribs for each instance
            int vertexAttribArrayIndex = 5;
            GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
            GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
            GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
            GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
            GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
            if (this.UVOffsetIsInstanced)
            {
                GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
            }
            GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
        }

        if (this.NeedsUpload && newBuffer)
        {
            int vertexAttribDivisorIndex = 5;

            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            if (this.UVOffsetIsInstanced)
            {
                GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            }
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
        }

        if (this.NeedsUpload)
        {
            if (newBuffer)
            {
                GL.BufferData(BufferTarget.ArrayBuffer,
                    sizeof(float) * this.Buffer.Length,
                    this.Buffer, BufferUsageHint.DynamicDraw);
            }
            else
            {
                GL.BufferSubData(BufferTarget.ArrayBuffer, 0,
                    sizeof(float) * this.Buffer.Length,
                    this.Buffer);
            }

            this.NeedsUpload = false;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }
}