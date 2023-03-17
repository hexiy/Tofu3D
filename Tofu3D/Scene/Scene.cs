using System.IO;
using System.Threading.Tasks;
using Engine;
using Tofu3D.Physics;
using Tofu3D.Rendering;
using Tofu3D.Tweening;

namespace Tofu3D;

public class Scene
{
	SceneLightingManager _sceneLightingManager;
	SceneRenderQueue _sceneRenderQueue;

	public List<GameObject> GameObjects = new();

	public string ScenePath = "";

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

	public void Initialize()
	{
		// PhysicsController.Init();
		_sceneLightingManager = new SceneLightingManager(this);
		_sceneRenderQueue = new SceneRenderQueue(this);

		RenderPassSystem.RegisterRender(RenderPassType.Opaques, RenderScene);

		if (SceneSerializer.LastScene != "" && File.Exists(SceneSerializer.LastScene))
		{
			LoadScene(SceneSerializer.LastScene);
		}
		else
		{
			CreateDefaultObjects();
		}
	}

	public void Update()
	{
		Debug.StartGraphTimer("Scene Update", DebugGraphTimer.SourceGroup.Update, TimeSpan.FromSeconds(1f / 60f));

		_sceneLightingManager.Update();
		_sceneRenderQueue.Update();
		
		Camera.I.GameObject.Update();
		TransformHandle.I.GameObject.Update();


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

		Debug.EndGraphTimer("Scene Update");
	}

	public static Action<Component> AnyComponentAddedToScene;

	public void OnComponentAdded(GameObject gameObject, Component component)
	{
		
		AnyComponentAddedToScene?.Invoke(component);
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


		SceneSerializer.LastScene = path;
		Window.I.Title = Window.I.WindowTitleText + " | " + Path.GetFileNameWithoutExtension(path);

		//Add method to clean scene
		while (GameObjects.Count > 0)
		{
			GameObjects[0].Destroy();
		}

		GameObjects.Clear();

		//Physics.rigidbodies.Clear();

		GameObjects = new List<GameObject>();
		SceneFile sceneFile = SceneSerializer.I.LoadGameObjects(path);

		Debug.StartTimer("ConnectGameObjectsWIthComponents");
		SceneSerializer.I.ConnectGameObjectsWithComponents(sceneFile);
		IDsManager.GameObjectNextId = sceneFile.GameObjectNextId + 1;
		Debug.EndAndLogTimer("ConnectGameObjectsWIthComponents");

		SceneSerializer.I.ConnectParentsAndChildren(sceneFile);
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
		path = path ?? SceneSerializer.LastScene;
		if (path.Length < 1)
		{
			path = Path.Combine("Assets", "scene1.scene");
		}

		SceneSerializer.LastScene = path;
		SceneSerializer.I.SaveGameObjects(GetSceneFile(), path);
	}

	public void CreateEmptySceneAndOpenIt(string path)
	{
		IDsManager.GameObjectNextId = 0;
		SceneSerializer.LastScene = path;
		GameObjects = new List<GameObject>();
		CreateDefaultObjects();
		SceneSerializer.I.SaveGameObjects(GetSceneFile(), path);
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