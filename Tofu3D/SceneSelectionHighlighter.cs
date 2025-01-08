using System.IO;
using Tofu3D.Tweening;

namespace Tofu3D;

public class SceneSelectionHighlighter
{
    private GameObject _selectionBoxGameObject;
    private List<GameObject> _selectedGameObjects = new List<GameObject>();
    
    public void Init()
    {
        GameObjectSelectionManager.GameObjectsSelected += OnGameObjectsSelected;
        Scene.SceneLoaded += SpawnSelectionBoxGameobjects;
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
        modelRenderer.MousePickingEnabled = false;

        Asset_Material runtimeMaterial = Tofu.AssetLoadManager
            .Load<Asset_Material>(TofuPath.Combine(Folders.MaterialsInAssets, "ModelRendererInstanced.mat"));
        runtimeMaterial = Tofu.AssetLoadManager.CreateCopyFile(runtimeMaterial);

        modelRenderer.Material = runtimeMaterial;
        modelRenderer.Material.NoDepth = true;
        modelRenderer.Material.IgnoreDepth = true;

        PremadeComponentSetupsHelper.PrepareCube(modelRenderer);

        runtimeMaterial.AlbedoTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(new AssetLoadParameters_Texture()
            { PathToAssetInLibrary = "Resources/whitePixel.png" });
        runtimeMaterial.Smoothness = 0;
        runtimeMaterial.AlbedoTint = new Color(1, 1, 1, 0.45f);
        runtimeMaterial.RenderMode = RenderMode.Transparent;
        runtimeMaterial.BlendMode = BlendMode.Fade;

        Tweener.Kill(this);
        Tweener.Tween(0.45f, 0.3f, 1.8f, (f) =>
            {
                // runtimeMaterial.AlbedoTint doesnt do anything... this only works when referencing material like this "modelRenderer.Material"
                // because in SetDefaultMaterial in renderer we created runtime copy, i'll keep this directly referencing modelRednerer.material so it doesnt break in future...
                modelRenderer.Material.AlbedoTint = modelRenderer.Material.AlbedoTint.SetA(f);
                // Debug.Log(modelRenderer.Material.AlbedoTint.A);
            }).SetTarget(this)
            .SetLoop(Tween.LoopType.Yoyo);

        _selectionBoxGameObject.Awake();
    }

    private void SetTransform()
    {
        if (_selectedGameObjects.Count == 0)
        {
            _selectionBoxGameObject.SetActive(false);
            return;
        }

        _selectionBoxGameObject.SetActive(true);

        GameObject go = _selectedGameObjects[0];

        if (go.ActiveInHierarchy == false)
        {
            // _selectionBoxGameObject.SetActive(false);
            // return;
        }

        go.GetComponent<BoxShape>(out BoxShape boxShape);
        if (boxShape != null)
        {
            _selectionBoxGameObject.GetComponent<BoxShape>().Size = boxShape.Size;
        }

        _selectionBoxGameObject.Transform.Pivot = go.Transform.Pivot;
        _selectionBoxGameObject.Transform.WorldPosition = go.Transform.WorldPosition;
        _selectionBoxGameObject.Transform.WorldScale = go.Transform.WorldScale + new Vector3(0.1f);
        _selectionBoxGameObject.Transform.Rotation = go.Transform.Rotation;
    }

    public void Update()
    {
        SetTransform();
    }
}