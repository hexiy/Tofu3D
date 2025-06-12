using System.Linq;
using TofuEngine.Rendering.Instancing;

public class SpriteRenderer : ModelRenderer
{
    [ShowIf(nameof(HasRectTransform))]
    [JsonIgnore]
    public bool KeepAspectRatio
    {
        get { return RectTransform?.AspectRatio != null; }
        set
        {
            if (RectTransform == null)
            {
                return;
            }

            if (value == true)
            {
                RectTransform.AspectRatio = (float)Texture.Size.X / Texture.Size.Y;
            }
            else
            {
                RectTransform.AspectRatio = null;
            }
        }
    }

    [ShowIf(nameof(HasRectTransform))]
    public Action SetNativeSize => SetNativeSizeMethod;

    private bool HasRectTransform => RectTransform != null;

    private void SetNativeSizeMethod()
    {
        RectTransform.Size = Texture.Size;
    }

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