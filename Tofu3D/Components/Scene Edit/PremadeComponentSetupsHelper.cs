using System.IO;
using System.Linq;

namespace Tofu3D;

public static class PremadeComponentSetupsHelper
{
    public static ModelRendererInstanced PrepareCube(ModelRendererInstanced modelRenderer)
    {
        // modelRenderer.Material= Tofu.AssetManager.Load<Asset_Material>("ModelSolid");adasdadasd
        // modelRenderer.Material.AlbedoTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(Path.Combine(Folders.TexturesInAssets, "solidColor.png"));
        Asset_Model model =
            Tofu.AssetLoadManager.Load<Asset_Model>(Path.Combine(Folders.ModelsInAssets, "defaultCube.obj"));
        modelRenderer.RuntimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(model.PathsToMeshAssets.First());

        return modelRenderer;
    }
}