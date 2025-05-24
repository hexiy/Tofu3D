using System.Linq;
using TofuEngine.Rendering.Instancing;

public class SpriteRenderer : ModelRenderer
{
    [Hide]
    public override RuntimeMesh RuntimeMesh { get; set; }

    [Show]
    public RuntimeTexture? Texture
    {
        get { return Material.AlbedoTexture; }
        set
        {
            if (Material == null)
            {
                return;
            }

            Material.AlbedoTexture = value;
        }
    }

    public override void Awake()
    {
        ObjectInstancingData = new ObjectInstancingData();

        base.Awake();
    }

    public override void SetupMeshAndMaterial()
    {
        base.SetupMeshAndMaterial();

        Asset_Model model =
            Tofu.AssetLoadManager.Get<Asset_Model>(TofuPath.Combine(Folders.BasicModelsInAssets, "plane.obj"));
        RuntimeMesh = Tofu.AssetLoadManager.Get<RuntimeMesh>(model.PathsToMeshAssets.First());

        Material = Tofu.AssetLoadManager.CreateCopyFile(Material);
        Material.LoadShader();
    }
}