namespace TofuEngine;

public class BasicMeshesCollection
{
    public RuntimeMesh RenderTextureMesh;
    public RuntimeMesh CubemapMesh;
    // public RuntimeMesh PlaneMesh;

    public BasicMeshesCollection()
    {
        RenderTextureMesh = new RuntimeMesh();
        BufferFactory.CreateRenderTextureBuffers(ref RenderTextureMesh.Vao);

        CubemapMesh = new RuntimeMesh();
        BufferFactory.CreateCubemapBuffers(ref CubemapMesh.Vao);

        // PlaneMesh = new RuntimeMesh();
        // BufferFactory.CreatePlaneMesh(ref PlaneMesh);
    }
}