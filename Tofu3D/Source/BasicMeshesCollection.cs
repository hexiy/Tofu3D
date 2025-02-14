namespace Tofu3D;

public class BasicMeshesCollection
{
    public RuntimeMesh RenderTextureMesh;
    public RuntimeMesh CubemapMesh;

    public BasicMeshesCollection()
    {
        RenderTextureMesh = new RuntimeMesh();
        BufferFactory.CreateRenderTextureBuffers(ref RenderTextureMesh.Vao);

        CubemapMesh = new RuntimeMesh();
        BufferFactory.CreateCubemapBuffers(ref CubemapMesh.Vao);
    }
}