/*using Tofu3D.Rendering.Instancing;

public static class StaticGeometryBaker
{
    public static int VAO = -1;
    public static int VBO = -1;
    private static int EBO = -1;
    public static float[] GeometryBuffer = [];
    public static float[] InstancedBuffer = [];
    public static HashSet<MeshVertexDataHashCode> BakedMeshesCodes = new HashSet<MeshVertexDataHashCode>();

    private const int _instancedVertexDataSizeInBytes =
        sizeof(float) * 3 * 4 // 4x vec 3's for model_1 model_2 model_3 model_4
        + (sizeof(float)) // 1 float(int) for mouse picking id
        + (sizeof(float) * 4) // 4 floats for albedo texture bounding box in atlas
        + (sizeof(float));

    public static int VerticesCount => GeometryBuffer.Length / 14;
    public static int InstancesCount => BakedMeshesCodes.Count;


    public static void AddMesh(Mesh mesh, Renderer renderer, InstancedRenderingBufferParameters bufferParameters)
    {
        if (mesh.VertexBufferData.Length == 0)
        {
            return;
        }
        MeshVertexDataHashCode hashCode = new MeshVertexDataHashCode(mesh, renderer.GameObjectId);
        if (BakedMeshesCodes.Contains(hashCode))
        {
            return;
        }

        bufferParameters.StartingIndexInInstancedBuffer = InstancedBuffer.Length;


        int newGeometryBufferDataArraySize = GeometryBuffer.Length + mesh.VertexBufferData.Length;
        Array.Resize(ref GeometryBuffer, newGeometryBufferDataArraySize);
        Array.Copy(sourceArray: mesh.VertexBufferData, destinationArray: GeometryBuffer, sourceIndex: 0,
            destinationIndex: newGeometryBufferDataArraySize - mesh.VertexBufferData.Length,
            length: mesh.VertexBufferData.Length);

        BakedMeshesCodes.Add(hashCode);

        CreateGeometryBuffer();

        int newInstancedBufferSize = InstancedBuffer.Length + (_instancedVertexDataSizeInBytes / sizeof(float));
        Array.Resize(ref InstancedBuffer, newInstancedBufferSize);
        InstancedRenderingSystem.CopyObjectDataToInstancedBuffer(ref InstancedBuffer, bufferParameters);
        Debug.Log($"Added new mesh to static buffer, number of meshes:{BakedMeshesCodes.Count}");


        UploadInstancedBuffer();
    }

    private static void CreateGeometryBuffer()
    {
        int[] countsOfElements = { 3, 2, 3, 3, 3 }; // position, uv, normal, tangent, bitangent
        BufferFactory.CreateGeometryBuffer(ref VAO, ref EBO, GeometryBuffer,
            countsOfElements);
    }


    public static void UploadInstancedBuffer()
    {
        Tofu.ShaderManager.BindVertexArray(VAO);


        var newBuffer = VBO == -1;
        if (newBuffer)
        {
            VBO = GL.GenBuffer();
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

        {
            // https://stackoverflow.com/a/28597384
            int offset = 0;
            int vertexAttribPointerIndex = 5;
            GL.VertexAttribPointer(vertexAttribPointerIndex++, 3, VertexAttribPointerType.Float, false,
                _instancedVertexDataSizeInBytes,
                offset);
            offset += 3 * sizeof(float);
            GL.VertexAttribPointer(vertexAttribPointerIndex++, 3, VertexAttribPointerType.Float, false,
                _instancedVertexDataSizeInBytes,
                offset);
            offset += 3 * sizeof(float);

            GL.VertexAttribPointer(vertexAttribPointerIndex++, 3, VertexAttribPointerType.Float, false,
                _instancedVertexDataSizeInBytes,
                offset);
            offset += 3 * sizeof(float);

            GL.VertexAttribPointer(vertexAttribPointerIndex++, 3, VertexAttribPointerType.Float, false,
                _instancedVertexDataSizeInBytes,
                offset);
            offset += 3 * sizeof(float);

            GL.VertexAttribPointer(vertexAttribPointerIndex++, 1, VertexAttribPointerType.Float, false,
                _instancedVertexDataSizeInBytes,
                offset);
            offset += 1 * sizeof(float);


            // albedo bounding box in atlas
            GL.VertexAttribPointer(vertexAttribPointerIndex++, 4, VertexAttribPointerType.Float, false,
                _instancedVertexDataSizeInBytes,
                offset);
            offset += 4 * sizeof(float);

            // atlas index of albedo texture
            GL.VertexAttribPointer(vertexAttribPointerIndex++, 1, VertexAttribPointerType.Float, false,
                _instancedVertexDataSizeInBytes,
                offset);
            offset += 1 * sizeof(float);
        }

        // unique attribs for each instance
        int vertexAttribArrayIndex = 5;
        GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
        GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
        GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
        GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
        GL.EnableVertexAttribArray(vertexAttribArrayIndex++);
        GL.EnableVertexAttribArray(vertexAttribArrayIndex++); // albedo texture bounds in atlas
        GL.EnableVertexAttribArray(vertexAttribArrayIndex++); // atlas index of albedo texture

        if (newBuffer)
        {
            int vertexAttribDivisorIndex = 5;

            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
            GL.VertexAttribDivisor(vertexAttribDivisorIndex++, 1);
        }

        GL.BufferData(BufferTarget.ArrayBuffer,
            sizeof(float) * InstancedBuffer.Length,
            InstancedBuffer, BufferUsageHint.StaticDraw);
    }
}*/