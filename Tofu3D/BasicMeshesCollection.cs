namespace Tofu3D;

public class BasicMeshesCollection
{
    public RuntimeMesh RenderTextureMesh;
    public RuntimeMesh CubemapMesh;

    public BasicMeshesCollection()
    {
        RenderTextureMesh = new RuntimeMesh() { VerticesCount = 24 };
        BufferFactory.CreateRenderTextureBuffers(ref RenderTextureMesh.Vao);
        
        CubemapMesh = new RuntimeMesh(){ VerticesCount = 24 };
        BufferFactory.CreateCubemapBuffers(ref CubemapMesh.Vao);
    }
}