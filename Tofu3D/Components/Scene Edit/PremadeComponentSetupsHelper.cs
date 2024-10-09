using System.IO;

namespace Tofu3D;

public static class PremadeComponentSetupsHelper
{
    public static ModelRendererInstanced PrepareCube(ModelRendererInstanced modelRenderer)
    {
        //modelRenderer.material.path
        // modelRenderer.Material= Tofu.AssetManager.Load<Asset_Material>("ModelSolid");adasdadasd
        // modelRenderer.Material.AlbedoTexture = Tofu.AssetManager.Load<Texture>(Path.Combine(Folders.Textures, "solidColor.png"));
        // modelRenderer.RuntimeMesh = Tofu.AssetManager.Load<RuntimeMesh>(Path.Combine(Folders.Models, "cube.obj"));

        return modelRenderer;
    }
}