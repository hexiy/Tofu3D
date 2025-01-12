using System.IO;

namespace Tofu3D;

public class Scene
{
    public static Action SceneLoaded = () => { };
    public static Action SceneStartedDisposing = () => { };
    public static Action SceneDisposed = () => { };
    public static Action<Component> ComponentAwoken = component => { };
    public static Action<Component> ComponentRemoved = component => { };
    public static Action<Component> ComponentEnabled = component => { };

    public static Action<Component> ComponentDisabled = component => { };

    // public static Action SceneModified = () => { };

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
    public string SceneName => Path.GetFileNameWithoutExtension(ScenePath);
    private Camera Camera => Camera.MainCamera;

    public string ThumbnailPath => GetThumbnailPath(ScenePath);

    public static string GetThumbnailPath(string scenePath)
    {
        string name = Path.GetFileNameWithoutExtension(scenePath);
        string thumbnailPath = TofuPath.Combine(Folders.SceneThumbnailsInLibrary, name + ".png.tofutexture");
        return thumbnailPath;
    }

    public void Initialize()
    {
        _sceneLightingManager = new SceneLightingManager(this);
        SceneFogManager = new SceneFogManager(this);
        _renderableComponentQueue = new RenderableComponentQueue();
        _updateableComponentQueue = new UpdateableComponentQueue();

        Tofu.RenderPassSystem.RegisterRender(RenderPassType.ZPrePass, RenderOpaques);
        Tofu.RenderPassSystem.RegisterRender(RenderPassType.Opaques, RenderOpaques);
        Tofu.RenderPassSystem.RegisterRender(RenderPassType.Transparency, RenderTransparency);
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
        Tofu.RenderPassSystem.RemoveRender(RenderPassType.ZPrePass, RenderOpaques);
        Tofu.RenderPassSystem.RemoveRender(RenderPassType.Opaques, RenderOpaques);
        Tofu.RenderPassSystem.RemoveRender(RenderPassType.Transparency, RenderTransparency);

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
        CreateLights();
    }

    private void CreateCamera()
    {
        if (FindComponent<Camera>(out Camera camera) == null)
        {
            var camGo = GameObject.Create(name: "Camera");
            camGo.AddComponent<Camera>();
            camGo.AddComponent<Skybox>();
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
        var transformHandleGameObject = GameObject.Create(visibleInHierarchy: false, runtimeOnly: true);
        TransformHandle = transformHandleGameObject.AddComponent<TransformHandle>();
        transformHandleGameObject.RuntimeOnly = true;
        transformHandleGameObject.AlwaysUpdate = true;
        transformHandleGameObject.Name = "Transform Handle";
        transformHandleGameObject.SetActive(false);
        transformHandleGameObject.Awake();

        transformHandleGameObject.SetActive(true);
    }


    private void CreateLights()
    {
        if (FindComponent<AmbientLight>() == null)
        {
            var ambientLightGo = GameObject.Create(name: "Ambient Light");
            AmbientLight ambientLight = ambientLightGo.AddComponent<AmbientLight>();
            ambientLight.Color = new Color(255, 219, 105, 255);
            ambientLight.Intensity = 0.34f;
            ambientLightGo.Awake();
        }

        if (FindComponent<DirectionalLight>() == null)
        {
            var directionLightGo = GameObject.Create(name: "Directional Light");
            directionLightGo.Transform.Rotation = new Vector3(90, 0, 0);
            DirectionalLight directionalLight = directionLightGo.AddComponent<DirectionalLight>();
            directionalLight.Color = new Color(255, 219, 105, 255);
            directionalLight.Intensity = 0.88f;
            directionLightGo.Awake();
        }

        if (FindComponent<PointLight>() == null)
        {
            var pointLightGo = GameObject.Create(name: "Point Light");
            PointLight pointLight = pointLightGo.AddComponent<PointLight>();
            pointLightGo.Awake();
        }
    }


    public void Update()
    {
        Debug.StartGraphTimer("Scene Update", DebugGraphTimer.SourceGroup.Update, TimeSpan.FromSeconds(1f / 120f));

        _sceneLightingManager.Update();
        // SceneFogManager.Update();

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

    // public void RenderAll()
    // {
    //     SetOpenGLState();
    //
    //     GL.ClearDepth(1000);
    //     GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
    //     _renderableComponentQueue.RenderAll();
    //     Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.All);
    //
    //     RestoreOpenGLState();
    // }

    public void RenderOpaques()
    {
        SetOpenGLState();

        // GL.ClearDepth(1000);
        // GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        _renderableComponentQueue.RenderOpaques();
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.Opaque);

        RestoreOpenGLState();
    }

    public void RenderTransparency()
    {
        // GL.Disable(EnableCap.CullFace);

        // GL.ClearDepth(1000);
        // GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        _renderableComponentQueue.RenderTransparency();
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.Transparent);
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

            if (GameObjects[i].RuntimeOnly)
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

    public T? FindComponent<T>(bool ignoreInactive = false) where T : Component
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

    public T? FindComponent<T>(out T component, bool ignoreInactive = false) where T : Component
    {
        foreach (var gameObject in GameObjects)
        {
            Component bl = gameObject.GetComponent<T>();
            if (bl != null && ((ignoreInactive && bl.IsActive) || ignoreInactive == false))
            {
                component = (T)bl;
                return component;
            }
        }

        component = null;
        return component;
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

    public GameObject GetGameObjectByID(int id)
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

    public List<GameObject> GetGameObjectsByIDs(List<int> ids)
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
        Tofu.SceneManager.LastOpenedSceneName = path;
        GameObjects = new List<GameObject>();
        //CreateDefaultObjects();
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