using System.IO;

namespace Tofu3D;

public class Scene
{
    public static Action SceneStartedDisposing = () => { };
    public static Action SceneDisposed = () => { };
    public static Action<Component> ComponentAwoken = component => { };
    public static Action<Component> ComponentRemoved = component => { };
    public static Action<Component> ComponentEnabled = component => { };

    public static Action<Component> ComponentDisabled = component => { };

    // public static Action SceneModified = () => { };
    public static Action AnySceneLoaded = () => { };

    private RenderableComponentQueue _renderableComponentQueue;
    private SceneLightingManager _sceneLightingManager;
    private UpdateableComponentQueue _updateableComponentQueue;

    // List<GameObject> _gameObjects = new();
    public List<GameObject> GameObjects = new();

    /*{
        get
        {
            lock (_gameObjects)
            {
                return _gameObjects;
            }
        }
        set
        {
            lock (_gameObjects)
            {
                _gameObjects = value;
            }
        }
    }*/
    public string ScenePath = "";

    public TransformHandle TransformHandle;
    public SceneFogManager SceneFogManager { get; private set; }
    public string SceneName => Path.GetFileName(ScenePath);
    private Camera Camera => Camera.MainCamera;

    public void Initialize()
    {
        _sceneLightingManager = new SceneLightingManager(this);
        SceneFogManager = new SceneFogManager(this);
        _renderableComponentQueue = new RenderableComponentQueue();
        _updateableComponentQueue = new UpdateableComponentQueue();

        Tofu.RenderPassSystem.RegisterRender(RenderPassType.ZPrePass, RenderWorld);
        Tofu.RenderPassSystem.RegisterRender(RenderPassType.Opaques, RenderWorld);
        // RenderPassSystem.RegisterRender(RenderPassType.Transparency, RenderTransparent);
    }

    public void DisposeScene()
    {
        SceneStartedDisposing.Invoke();
        // while (GameObjects.Count > 0)
        // {
        // 	GameObjects[0].Destroy();
        // }

        foreach (var gameObject in GameObjects)
        {
            gameObject.SetActive(false);
        }
        // while (GameObjects.Count > 0)
        // {
        // 	GameObjects[0].Destroy();
        // }

        // GameObjects.Clear();
        // GameObjects = new List<GameObject>();
        Tofu.RenderPassSystem.RemoveRender(RenderPassType.ZPrePass, RenderWorld);
        Tofu.RenderPassSystem.RemoveRender(RenderPassType.Opaques, RenderWorld);
        // RenderPassSystem.RemoveRender(RenderPassType.UI, RenderUI);
        Tofu.InstancedRenderingSystem.ClearBuffers();

        SceneDisposed.Invoke();
    }

    // public void ForceRenderQueueChanged()
    // {
    // 	_renderableComponentQueue.RenderQueueChanged();
    // }

    public void CreateDefaultObjects()
    {
        CreateCamera();
        CreateTransformHandle();
        CreateGrid();
    }

    private void CreateCamera()
    {
        if (FindComponent(typeof(Camera)) == null)
        {
            var camGo = GameObject.Create(name: "Camera");
            camGo.AddComponent<Camera>();
            camGo.Awake();
        }
    }

    private void CreateGrid()
    {
        return;
        // var gridGameObject = GameObject.Create(silent: true);
        // gridGameObject.AddComponent<Grid>();
        // gridGameObject.DynamicallyCreated = true;
        // gridGameObject.AlwaysUpdate = true;
        // gridGameObject.Name = "Grid";
        // gridGameObject.Awake();
        // gridGameObject.Start();
    }

    private void CreateTransformHandle()
    {
        var transformHandleGameObject = GameObject.Create(silent: true);
        TransformHandle = transformHandleGameObject.AddComponent<TransformHandle>();
        transformHandleGameObject.DynamicallyCreated = true;
        transformHandleGameObject.AlwaysUpdate = true;
        transformHandleGameObject.Name = "Transform Handle";
        transformHandleGameObject.SetActive(false);
        transformHandleGameObject.Awake();
        transformHandleGameObject.Start();
    }

    public void Update()
    {
        Debug.StartGraphTimer("Scene Update", DebugGraphTimer.SourceGroup.Update, TimeSpan.FromSeconds(1f / 120f));

        _sceneLightingManager.Update();
        SceneFogManager.Update();

        // Camera.MainCamera.GameObject.Update();
        // TransformHandle.I.GameObject.Update();

        _updateableComponentQueue.UpdateComponents();

        // for (int i = 0; i < GameObjects.Count; i++)
        // {
// 			/*if (GameObjects[i] == Camera.MainCamera.GameObject)
// 			{
// 				continue;
// 			}
//
// 			if (GameObjects[i] == TransformHandle.I.GameObject)
// 			{
// 				continue;
// 			}
//
// 			if (Global.GameRunning || GameObjects[i].AlwaysUpdate)
// 			{
// 				GameObjects[i].Update();
// 				GameObjects[i].FixedUpdate();
// 			}
// 			else if (Global.GameRunning == false)
// 			{
// 				//gameObjects[i].EditorUpdate();*/
// 			GameObjects[i].Update();
// 			// }
// 		}

        Debug.EndGraphTimer("Scene Update");
    }

    // ReSharper disable once InconsistentNaming
    private void SetOpenGLState()
    {
        // GL.Enable(EnableCap.DepthTest);
        // GL.DepthFunc(DepthFunction.Lequal);

        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);
        GL.FrontFace(FrontFaceDirection.Cw);
    }

    // ReSharper disable once InconsistentNaming
    private void RestoreOpenGLState()
    {
        GL.Disable(EnableCap.CullFace);
    }

    public void RenderWorld()
    {
        SetOpenGLState();


        GL.ClearDepth(1000);
        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        // //
        // //
        // GL.Viewport(0, 0, (int) Camera.MainCamera.Size.X, (int) Camera.MainCamera.Size.Y);

        _renderableComponentQueue.RenderWorld();
        Tofu.InstancedRenderingSystem.RenderInstances();

        // if (TransformHandle.Transform.IsInCanvas == false)
        // {
        // 	TransformHandle.I.GameObject.Render();
        // }


        RestoreOpenGLState();
    }

    // public void RenderUI()
    // {
    // 	// GL.Enable(EnableCap.DepthTest);
    // 	// GL.DepthFunc(DepthFunction.Less);
    // 	// GL.Enable(EnableCap.StencilTest);
    //
    // 	_renderableComponentQueue.RenderUI();
    //
    // 	if (TransformHandle.Transform.IsInCanvas)
    // 	{
    // 		TransformHandle.I.GameObject.Render();
    // 	}
    // }

    // public void RenderTransparent()
    // {
    // 	_sceneRenderQueue.RenderTransparent();
    // }

    public SceneFile GetSceneFile()
    {
        SceneFile sf = new();
        sf.Components = new List<Component>();
        sf.GameObjects = new List<GameObject>();
        for (var i = 0; i < GameObjects.Count; i++)
        {
            GameObjects[i].IndexInHierarchy = i;

            if (GameObjects[i].DynamicallyCreated)
            {
                continue;
            }

            sf.Components.AddRange(GameObjects[i].Components);
            sf.GameObjects.Add(GameObjects[i]);
        }

        sf.GameObjectNextId = IDsManager.GameObjectNextId;
        return sf;
    }

    public GameObject FindComponent(Type type)
    {
        foreach (var gameObject in GameObjects)
        {
            var bl = gameObject.GetComponent(type);
            if (bl != null)
            {
                return gameObject;
            }
        }

        return null;
    }

    public T FindComponent<T>(bool ignoreInactive = false) where T : Component
    {
        foreach (var gameObject in GameObjects)
        {
            Component bl = gameObject.GetComponent<T>();
            if (bl != null && ((ignoreInactive && bl.IsActive) || ignoreInactive == false))
            {
                return (T)bl;
            }
        }

        return null;
    }

    public List<T> FindComponentsInScene<T>(bool ignoreInactive = false) where T : Component
    {
        List<T> components = new();
        foreach (var gameObject in GameObjects)
        {
            var bl = gameObject.GetComponent<T>();
            if (bl != null)
            {
                if (ignoreInactive && (bl.Enabled == false || bl.GameObject.ActiveSelf == false))
                {
                    continue;
                }

                components.Add(bl);
            }
        }

        return components;
    }

    public GameObject GetGameObject(int id)
    {
        for (var i = 0; i < GameObjects.Count; i++)
        {
            if (GameObjects[i].Id == id)
            {
                return GameObjects[i];
            }
        }

        return null;
    }

    public List<GameObject> GetGameObjects(List<int> ids)
    {
        List<GameObject> foundGameObjects = new();
        for (var i = 0; i < GameObjects.Count; i++)
        {
            if (ids.Contains(GameObjects[i].Id))
            {
                foundGameObjects.Add(GameObjects[i]);
            }
        }

        return foundGameObjects;
    }

    public void AddGameObjectToScene(GameObject gameObject)
    {
        GameObjects.Add(gameObject);
        UpdateGameobjectsIndexInHierarchy();
        // _renderableComponentQueue.RenderQueueChanged();
    }

    public void AddGameObjectsToScene(IEnumerable<GameObject> gameObjects)
    {
        GameObjects.AddRange(gameObjects);
        UpdateGameobjectsIndexInHierarchy();
        // _renderableComponentQueue.RenderQueueChanged();
    }

    public void SetupAndSaveEmptyScene(string path)
    {
        IDsManager.GameObjectNextId = 0;
        Tofu.SceneManager.LastOpenedScene = path;
        GameObjects = new List<GameObject>();
        CreateDefaultObjects();
        Tofu.SceneSerializer.SaveGameObjects(GetSceneFile(), path);
    }

    public void OnGameObjectDestroyed(GameObject gameObject)
    {
        if (GameObjects.Contains(gameObject))
        {
            GameObjects.Remove(gameObject);
        }

        UpdateGameobjectsIndexInHierarchy();
        // SceneModified.Invoke();
    }

    public void UpdateGameobjectsIndexInHierarchy()
    {
        for (var i = 0; i < GameObjects.Count; i++)
        {
            GameObjects[i].IndexInHierarchy = i;
        }
    }
    private void OnMouse3Clicked()
    {
    }

    private void OnMouse3Released()
    {
    }
}