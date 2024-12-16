namespace Tofu3D;

public class SceneSelectionHighlighter
{
    private GameObject _selectionBoxGameObject;
    private List<GameObject> _selectedGameObjects = new List<GameObject>();

    public void Init()
    {
        GameObjectSelectionManager.GameObjectsSelected += OnGameObjectsSelected;
        SpawnSelectionBoxGameobjects();
    }

    private void OnGameObjectsSelected(List<GameObject> gameObjects)
    {
        _selectedGameObjects = gameObjects;

        SetTransform();
    }

    private void SpawnSelectionBoxGameobjects()
    {
        _selectionBoxGameObject = GameObject.Create(name: "Selection Box");


        BoxShape boxShape = _selectionBoxGameObject.AddComponent<BoxShape>();
        ModelRendererInstanced modelRenderer = _selectionBoxGameObject.AddComponent<ModelRendererInstanced>();
        PremadeComponentSetupsHelper.PrepareCube(modelRenderer);

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
        _selectionBoxGameObject.Transform.WorldScale = go.Transform.WorldScale;
        _selectionBoxGameObject.Transform.Rotation = go.Transform.Rotation;
    }

    public void Update()
    {
        SetTransform();
    }
}