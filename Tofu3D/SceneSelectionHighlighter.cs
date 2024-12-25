namespace Tofu3D;

public class SceneSelectionHighlighter
{
    private GameObject _selectionBoxGameObject;
    private List<GameObject> _selectedGameObjects = new List<GameObject>();

    public void Init()
    {
        GameObjectSelectionManager.GameObjectsSelected += OnGameObjectsSelected;
        SceneManager.SceneLoaded += SpawnSelectionBoxGameobjects;
        SpawnSelectionBoxGameobjects();
    }

    private void OnGameObjectsSelected(List<GameObject> gameObjects)
    {
        _selectedGameObjects = gameObjects;

        SetTransform();
    }

    private void SpawnSelectionBoxGameobjects()
    {
        _selectionBoxGameObject =
            GameObject.Create(name: "Selection Box", visibleInHierarchy: false, runtimeOnly: true);


        BoxShape boxShape = _selectionBoxGameObject.AddComponent<BoxShape>();
        ModelRendererInstanced modelRenderer = _selectionBoxGameObject.AddComponent<ModelRendererInstanced>();

        Asset_Material material = Tofu.AssetLoadManager
            .Load<Asset_Material>("Assets/Materials/ModelRendererInstanced.mat").CreateRuntimeCopy();

        modelRenderer.Material = material;

        PremadeComponentSetupsHelper.PrepareCube(modelRenderer);

        material.AlbedoTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(new AssetLoadParameters_Texture()
            { PathToAsset = "Resources/whitePixel.png" });
        material.Smoothness = 0;
        material.AlbedoTint = new Color(1, 1, 1, 0.75f);
        material.RenderMode = RenderMode.Transparent;
        material.BlendMode = BlendMode.Fade;

        _selectionBoxGameObject.Start();
    }

    private void SetTransform()
    {
        if (_selectedGameObjects.Count == 0)
        {
            return;
        }

        GameObject go = _selectedGameObjects[0];


        go.GetComponent<BoxShape>(out BoxShape boxShape);
        if (boxShape != null)
        {
            _selectionBoxGameObject.GetComponent<BoxShape>().Size = boxShape.Size;
        }

        _selectionBoxGameObject.Transform.WorldPosition = go.Transform.WorldPosition;
        _selectionBoxGameObject.Transform.WorldScale = go.Transform.WorldScale + new Vector3(0.1f);
        _selectionBoxGameObject.Transform.Rotation = go.Transform.Rotation;
    }

    public void Update()
    {
        SetTransform();
    }
}