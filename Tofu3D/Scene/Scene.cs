using System.IO;
using System.Threading.Tasks;
using Engine;
using Tofu3D.Physics;
using Tofu3D.Rendering;
using Tofu3D.Tweening;

namespace Tofu3D;

public class Scene
{
	public List<GameObject> GameObjects = new();

	List<Renderer> _renderQueue = new();

	public string ScenePath = "";

	public Scene()
	{
		I = this;
	}

	public static Scene I { get; private set; }

	Camera Camera
	{
		get { return Camera.I; }
	}

	void CreateDefaultObjects()
	{
		CreateCamera();
		CreateTransformHandle();
		CreateGrid();
	}

	void CreateCamera()
	{
		if (FindComponent(typeof(Camera)) == null)
		{
			GameObject camGo = GameObject.Create(name: "Camera");
			camGo.AddComponent<Camera>();
			camGo.Awake();
		}
	}

	void CreateGrid()
	{
		return;
		GameObject gridGameObject = GameObject.Create(silent: true);
		gridGameObject.AddComponent<Grid>();
		gridGameObject.DynamicallyCreated = true;
		gridGameObject.AlwaysUpdate = true;
		gridGameObject.Name = "Grid";
		gridGameObject.Awake();
		gridGameObject.Start();
	}

	void CreateTransformHandle()
	{
		GameObject transformHandleGameObject = GameObject.Create(silent: true);
		Editor.I.TransformHandle = transformHandleGameObject.AddComponent<TransformHandle>();
		transformHandleGameObject.DynamicallyCreated = true;
		transformHandleGameObject.AlwaysUpdate = true;
		transformHandleGameObject.Name = "Transform Handle";
		transformHandleGameObject.ActiveSelf = false;
		transformHandleGameObject.Awake();
		transformHandleGameObject.Start();
	}

	public void Start()
	{
		PhysicsController.Init();
		RenderPassSystem.RegisterRender(RenderPassType.Opaques, RenderScene);

		if (Serializer.LastScene != "" && File.Exists(Serializer.LastScene))
		{
			LoadScene(Serializer.LastScene);
		}
		else
		{
			CreateDefaultObjects();
		}
	}

	public void Update()
	{
		Time.Update();
		MouseInput.Update();
		TweenManager.I.Update();
		SceneNavigation.I.Update();
		ShaderCache.ReloadQueuedShaders();
		LightManager.I.Update();
		// MousePickingSystem.Update();
		Camera.I.GameObject.Update();

		TransformHandle.I.GameObject.Update();

		if (_renderQueueChanged)
		{
			RebuildRenderQueue();
			_renderQueueChanged = false;
		}
		else if (Time.ElapsedTicks % 20 == 0)
		{
			SortRenderQueue();
		}


		for (int i = 0; i < GameObjects.Count; i++)
		{
			GameObjects[i].IndexInHierarchy = i;
			if (GameObjects[i] == Camera.I.GameObject)
			{
				continue;
			}

			if (GameObjects[i] == TransformHandle.I.GameObject)
			{
				continue;
			}

			if (Global.GameRunning || GameObjects[i].AlwaysUpdate)
			{
				GameObjects[i].Update();
				GameObjects[i].FixedUpdate();
			}
			else if (Global.GameRunning == false)
			{
				//gameObjects[i].EditorUpdate();
				GameObjects[i].Update();
			}
		}
	}

	public static Action<Component> AnyComponentAddedToScene;
	bool _renderQueueChanged = false;

	public void OnComponentAdded(GameObject gameObject, Component component)
	{
		RenderQueueChanged();
		AnyComponentAddedToScene?.Invoke(component);
	}

	public void RenderQueueChanged()
	{
		_renderQueueChanged = true;
	}

	private void RebuildRenderQueue()
	{
		_renderQueue = new List<Renderer>();
		for (int i = 0; i < GameObjects.Count; i++)
		{
			if (GameObjects[i].GetComponent<Renderer>())
			{
				_renderQueue.AddRange(GameObjects[i].GetComponents<Renderer>());
			}
		}

		SortRenderQueue();
	}

	public void SortRenderQueue()
	{
		_renderQueue.Sort();
	}

	public void RenderScene()
	{
		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);
		GL.Enable(EnableCap.StencilTest);

		// GL.DepthFunc(DepthFunction.Greater);
		GL.ClearColor(Camera.Color.ToOtherColor());
		GL.ClearDepth(1000);


		GL.Viewport(0, 0, (int) Camera.I.Size.X, (int) Camera.I.Size.Y);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		//BatchingManager.RenderAllBatchers();
		for (int i = 0; i < _renderQueue.Count; i++)
		{
			if (_renderQueue[i].Enabled && _renderQueue[i].GameObject.Awoken && _renderQueue[i].GameObject.ActiveInHierarchy)
			{
				_renderQueue[i].UpdateMvp();
				_renderQueue[i].Render();
			}
		}

		//BatchingManager.RenderAllBatchers();

		TransformHandle.I.GameObject.Render();
	}

	public SceneFile GetSceneFile()
	{
		SceneFile sf = new();
		sf.Components = new List<Component>();
		sf.GameObjects = new List<GameObject>();
		for (int i = 0; i < GameObjects.Count; i++)
		{
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
		foreach (GameObject gameObject in GameObjects)
		{
			Component bl = gameObject.GetComponent(type);
			if (bl != null)
			{
				return gameObject;
			}
		}

		return null;
	}

	public T FindComponent<T>(bool ignoreInactive = false) where T : Component
	{
		foreach (GameObject gameObject in GameObjects)
		{
			Component bl = gameObject.GetComponent<T>();
			if (bl != null && ((ignoreInactive && bl.IsActive) || (ignoreInactive == false)))
			{
				return (T) bl;
			}
		}

		return null;
	}

	public List<T> FindComponentsInScene<T>(bool ignoreInactive = false) where T : Component
	{
		List<T> components = new();
		foreach (GameObject gameObject in GameObjects)
		{
			T bl = gameObject.GetComponent<T>();
			if (bl != null)
			{
				if (ignoreInactive == true && (bl.Enabled == false || bl.GameObject.ActiveSelf == false))
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
		for (int i = 0; i < GameObjects.Count; i++)
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
		List<GameObject> foundGameObjects = new List<GameObject>();
		for (int i = 0; i < GameObjects.Count; i++)
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

		RenderQueueChanged();
	}

	public bool LoadScene(string path = null)
	{
		Debug.ClearLogs();

		Debug.StartTimer("LoadScene");


		Serializer.LastScene = path;
		Window.I.Title = Window.I.WindowTitleText + " | " + Path.GetFileNameWithoutExtension(path);

		//Add method to clean scene
		while (GameObjects.Count > 0)
		{
			GameObjects[0].Destroy();
		}

		GameObjects.Clear();

		//Physics.rigidbodies.Clear();

		GameObjects = new List<GameObject>();
		SceneFile sceneFile = Serializer.I.LoadGameObjects(path);

		Serializer.I.ConnectGameObjectsWithComponents(sceneFile);
		IDsManager.GameObjectNextId = sceneFile.GameObjectNextId + 1;

		Serializer.I.ConnectParentsAndChildren(sceneFile);
		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			for (int j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
			{
				sceneFile.GameObjects[i].Components[j].GameObjectId = sceneFile.GameObjects[i].Id;
			}

			I.AddGameObjectToScene(sceneFile.GameObjects[i]);
		}

		Debug.StartTimer("Awake");
		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			//sceneFile.GameObjects[i].LinkGameObjectFieldsInComponents();
			sceneFile.GameObjects[i].Awake();
		}

		Debug.EndAndLogTimer("Awake");


		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			sceneFile.GameObjects[i].Start();
		}


		CreateDefaultObjects();

		ScenePath = path;

		int lastSelectedGameObjectId = PersistentData.GetInt("lastSelectedGameObjectId", 0);
		if (Global.EditorAttached)
		{
			EditorPanelHierarchy.I.SelectGameObject(lastSelectedGameObjectId);
		}


		Debug.EndAndLogTimer("LoadScene");

		return true;
	}

	public void SaveScene(string path = null)
	{
		path = path ?? Serializer.LastScene;
		if (path.Length < 1)
		{
			path = Path.Combine("Assets", "scene1.scene");
		}

		Serializer.LastScene = path;
		Serializer.I.SaveGameObjects(GetSceneFile(), path);
	}

	public void CreateEmptySceneAndOpenIt(string path)
	{
		IDsManager.GameObjectNextId = 0;
		Serializer.LastScene = path;
		GameObjects = new List<GameObject>();
		CreateDefaultObjects();
		Serializer.I.SaveGameObjects(GetSceneFile(), path);
	}

	public void OnGameObjectDestroyed(GameObject gameObject)
	{
		if (GameObjects.Contains(gameObject))
		{
			GameObjects.Remove(gameObject);
		}

		RenderQueueChanged();
	}

	void OnMouse3Clicked()
	{
	}

	void OnMouse3Released()
	{
	}

	public void DisposeScene()
	{
		foreach (GameObject gameObject in GameObjects)
		{
			foreach (Component component in gameObject.Components)
			{
				component.Dispose();
			}
		}
	}
}