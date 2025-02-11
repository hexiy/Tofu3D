using System.Linq;

namespace Tofu3D;

public static class PremadeComponentSetupsHelper
{
    public static ModelRendererInstanced PrepareCube(ModelRendererInstanced modelRenderer)
    {
        if (modelRenderer.Material == null)
        {
            modelRenderer.Material =
                Tofu.AssetLoadManager.Get<Asset_Material>(TofuPath.Combine(Folders.MaterialsInAssets,"ModelRendererInstanced.mat"));
        }

        modelRenderer.Material.AlbedoTexture = Tofu.Editor.EditorTextures.WhitePixel;
        Asset_Model model =
            Tofu.AssetLoadManager.Get<Asset_Model>(TofuPath.Combine(Folders.BasicModelsInAssets, "defaultCube.obj"));
        modelRenderer.RuntimeMesh = Tofu.AssetLoadManager.Get<RuntimeMesh>(model.PathsToMeshAssets.First());

        return modelRenderer;
    }
}