/*using TofuEngine.Rendering.Instancing;

public class StaticGeometryBaker
{
    public Dictionary<int, SharedInstancingBuffer> SharedInstancingBuffers =
        new Dictionary<int, SharedInstancingBuffer>();

    public float[] GeometryBuffer = [];
    public HashSet<MeshVertexDataHashCode> BakedMeshesCodes = new HashSet<MeshVertexDataHashCode>();

    public StaticGeometryBaker()
    {
    }


    private SharedInstancingBuffer GetOrCreateInstancingBufferByShader(Shader shader)
    {
        if (SharedInstancingBuffers.TryGetValue(shader.ProgramId, out SharedInstancingBuffer buffer))
        {
            return buffer;
        }
        else
        {
            SharedInstancingBuffer buff = new SharedInstancingBuffer(shader);
            buff.
            return SharedInstancingBuffers[shader.ProgramId] = ;
        }
    }

    public void AddMesh(Mesh mesh, Renderer renderer, InstancedRenderingBufferParameters bufferParameters)
    {
        if (mesh.GeometryBufferData.Length == 0)
        {
            return;
        }

        MeshVertexDataHashCode hashCode = new MeshVertexDataHashCode(mesh, renderer.GameObjectId);
        if (BakedMeshesCodes.Contains(hashCode))
        {
            return;
        }

        SharedInstancingBuffer InstancingBuffer = GetOrCreateInstancingBufferByShader(renderer.Material.Shader);

        bufferParameters.StartingIndexInInstancedBuffer = InstancingBuffer.Length;


        int newGeometryBufferDataArraySize = GeometryBuffer.Length + mesh.GeometryBufferData.Length;
        Array.Resize(ref GeometryBuffer, newGeometryBufferDataArraySize);
        Array.Copy(sourceArray: mesh.GeometryBufferData, destinationArray: GeometryBuffer, sourceIndex: 0,
            destinationIndex: newGeometryBufferDataArraySize - mesh.GeometryBufferData.Length,
            length: mesh.GeometryBufferData.Length);

        BakedMeshesCodes.Add(hashCode);

        CreateGeometryBuffer();

        int newInstancedBufferSize = InstancingBuffer.Length + (_instancedVertexDataSizeInBytes / sizeof(float));
        Array.Resize(ref InstancingBuffer, newInstancedBufferSize);

        Tofu.InstancedRenderingSystem.CopyObjectDataToBuffer(ref InstancingBuffer, bufferParameters);
        Debug.Log($"Added new mesh to static buffer, number of meshes:{BakedMeshesCodes.Count}");


        UploadInstancedBuffer();
    }

    private void CreateGeometryBuffer()
    {
        int[] countsOfElements = { 3, 2, 3, 3, 3 }; // position, uv, normal, tangent, bitangent
        BufferFactory.CreateGeometryBuffer(ref VAO, ref EBO, GeometryBuffer,
            countsOfElements);
    }
}*/