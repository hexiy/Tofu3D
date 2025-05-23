using System.IO;

[ExecuteInEditMode]
public class Grid : Component, IComponentUpdateable
{
    private BoxShape _boxShape;
    private ModelRendererInstanced _renderer;
    public Vector2 PanSpeed = Vector2.Zero;

    public void Update()
    {
        // float clampedOrthoSize = Mathf.ClampMin(Camera.I.OrthographicSize, 1);
        // _boxShape.Size = Camera.I.Size;
        // _spriteRenderer.Tiling = _boxShape.Size / 100f / (10 / Camera.I.OrthographicSize);
        // _spriteRenderer.Offset = Camera.MainCamera.Transform.WorldPosition * PanSpeed / _spriteRenderer.Tiling;

        // Transform.LocalScale = Vector3.One;
        // Transform.LocalPosition = Vector3.Zero;

        // Transform.WorldPosition = new Vector3(0, -10, 0);
        // // Transform.LocalScale = new Vector3(100, 1, 100);
        // Transform.LocalScale = new Vector3(3, 1, 3);
    }

    public override void Awake()
    {
        _boxShape = GetComponent<BoxShape>() ?? AddComponent<BoxShape>();
        _renderer = GetComponent<ModelRendererInstanced>() ?? AddComponent<ModelRendererInstanced>();


        _renderer.SetupMeshAndMaterial();
        _renderer.Material.AlbedoTexture =
            Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.TexturesInAssets, "gridX.png"));
        //,TextureLoadSettings.DefaultSettingsSpritePixelArt);
        _renderer.Color = new Color(255, 255, 255, 255);
        _renderer.Layer = -10;
        _renderer.Material.BlendMode = BlendMode.PremultipliedAlpha;


        _renderer.RuntimeMesh =
            Tofu.AssetLoadManager.Get<Asset_Model>(TofuPath.Combine(Folders.BasicModelsInAssets, "plane.obj"))
                .GetMesh(0);
        _renderer.NeedsToSetupMeshAndMaterial = false;

        Transform.WorldPosition = new Vector3(0, 0, 20);
        Transform.Rotation = new Vector3(90, 0, 0);
        Transform.LocalScale = new Vector3(5, 1, 5);
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }
}