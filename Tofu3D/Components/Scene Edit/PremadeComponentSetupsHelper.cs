using System.IO;
using System.Linq;

namespace Tofu3D;

public static class PremadeComponentSetupsHelper
{
    public static ModelRendererInstanced PrepareCube(ModelRendererInstanced modelRenderer)
    {
        modelRenderer.Material.AlbedoTexture = Tofu.Editor.EditorTextures.WhitePixel;
        Asset_Model model =
            Tofu.AssetLoadManager.Load<Asset_Model>(Path.Combine(Folders.ModelsInAssets, "defaultCube.obj"));
        modelRenderer.RuntimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(model.PathsToMeshAssets.First());

        return modelRenderer;
    }
}