using System.IO;
using Tofu3D.Physics;
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
		GameObject camGo = GameObject.Create(name: "Camera");
		camGo.AddComponent<Camera>();
		camGo.Awake();

		CreateTransformHandle();
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
		if (Time.ElapsedTicks % 20 == 0)
		{
			SortRenderQueue();
		}

		for (int i = 0; i < GameObjects.Count; i++)
		{
			GameObjects[i].IndexInHierarchy = i;
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

	public void OnComponentAdded(GameObject gameObject, Component component)
	{
		RenderQueueChanged();
	}

	public void RenderQueueChanged()
	{
		_renderQueue = new List<Renderer>();
		for (int i = 0; i < GameObjects.Count; i++)
		{
			if (GameObjects[i].GetComponent<Renderer>())
				//renderQueue.AddRange(gameObjects[i].GetComponents<Renderer>());
			{
				_renderQueue.AddRange(GameObjects[i].GetComponents<Renderer>());
			}
		}


		for (int i = 0; i < _renderQueue.Count; i++)
		{
			_renderQueue[i].LayerFromHierarchy = _renderQueue[i].GameObject.IndexInHierarchy * 0.00000000000000000000000000000001f;
		}

		SortRenderQueue();
	}

	public void SortRenderQueue()
	{
		_renderQueue.Sort();
	}

	public void Render()
	{
		GL.Enable(EnableCap.DepthTest);
		GL.Enable(EnableCap.CullFace);
		GL.Enable(EnableCap.StencilTest);
		GL.DepthFunc(DepthFunction.Greater);

		GL.ClearColor(Camera.Color.ToOtherColor());
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		//BatchingManager.RenderAllBatchers();

		for (int i = 0; i < _renderQueue.Count; i++)
		{
			if (_renderQueue[i].Enabled && _renderQueue[i].Awoken && _renderQueue[i].GameObject.ActiveInHierarchy)
			{
				if (_renderQueue[i].GameObject == TransformHandle.I.GameObject)
				{
					continue;
				}

				_renderQueue[i].Render();
			}
		}

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

		sf.GameObjectNextId = DsManager.GameObjectNextId;
		return sf;
	}

	public GameObject FindGameObject(Type type)
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

	public List<T> FindComponentsInScene<T>() where T : Component
	{
		List<T> components = new();
		foreach (GameObject gameObject in GameObjects)
		{
			T bl = gameObject.GetComponent<T>();
			if (bl != null)
			{
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

	public void AddGameObjectToScene(GameObject gameObject)
	{
		GameObjects.Add(gameObject);

		RenderQueueChanged();
	}

	public bool LoadScene(string path = null)
	{
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
		DsManager.GameObjectNextId = sceneFile.GameObjectNextId + 1;

		Serializer.I.ConnectParentsAndChildren(sceneFile);

		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			for (int j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
			{
				sceneFile.GameObjects[i].Components[j].GameObjectId = sceneFile.GameObjects[i].Id;
			}

			I.AddGameObjectToScene(sceneFile.GameObjects[i]);
		}

		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			sceneFile.GameObjects[i].LinkGameObjectFieldsInComponents();
			sceneFile.GameObjects[i].Awake();
			sceneFile.GameObjects[i].Start();
		}


		CreateTransformHandle();

		ScenePath = path;

		int lastSelectedGameObjectId = PersistentData.GetInt("lastSelectedGameObjectId", 0);
		if (Global.EditorAttached)
		{
			EditorPanelHierarchy.I.SelectGameObject(lastSelectedGameObjectId);
		}

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
		DsManager.GameObjectNextId = 0;
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
}