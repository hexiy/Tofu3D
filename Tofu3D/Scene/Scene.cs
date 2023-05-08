﻿using System.IO;
using System.Threading.Tasks;
using Tofu3D.Physics;
using Tofu3D.Rendering;
using Tofu3D.Tweening;

namespace Tofu3D;

public class Scene
{
	SceneLightingManager _sceneLightingManager;
	public SceneFogManager SceneFogManager { get; private set; }
	SceneRenderQueue _sceneRenderQueue;
	public TransformHandle TransformHandle;

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
	public string SceneName
	{
		get { return Path.GetFileName(ScenePath); }
	}

	public static Action SceneDisposed = () => { };
	public static Action<Component> ComponentAdded = component => { };
	public static Action SceneModified = () => { };
	public static Action AnySceneLoaded = () => { };
	Camera Camera
	{
		get { return Camera.MainCamera; }
	}

	public void Initialize()
	{
		_sceneLightingManager = new SceneLightingManager(this);
		SceneFogManager = new SceneFogManager(this);
		_sceneRenderQueue = new SceneRenderQueue(this);

		RenderPassSystem.RegisterRender(RenderPassType.Opaques, RenderWorld);
		RenderPassSystem.RegisterRender(RenderPassType.UI, RenderUI);
		// RenderPassSystem.RegisterRender(RenderPassType.Transparency, RenderTransparent);
	}

	public void DisposeScene()
	{
		while (GameObjects.Count > 0)
		{
			GameObjects[0].Destroy();
		}

		GameObjects.Clear();
		GameObjects = new List<GameObject>();
		RenderPassSystem.RemoveRender(RenderPassType.Opaques, RenderWorld);
		RenderPassSystem.RemoveRender(RenderPassType.UI, RenderUI);
		SceneDisposed.Invoke();
	}

	public void ForceRenderQueueChanged()
	{
		_sceneRenderQueue.RenderQueueChanged();
	}

	public void CreateDefaultObjects()
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
		Debug.StartGraphTimer("Scene Update", DebugGraphTimer.SourceGroup.Update, TimeSpan.FromSeconds(1f / 60f));

		_sceneLightingManager.Update();
		_sceneRenderQueue.Update();
		SceneFogManager.Update();

		Camera.MainCamera.GameObject.Update();
		TransformHandle.I.GameObject.Update();


		for (int i = 0; i < GameObjects.Count; i++)
		{
			GameObjects[i].IndexInHierarchy = i;
			if (GameObjects[i] == Camera.MainCamera.GameObject)
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

	public void OnComponentAdded(Component component)
	{
		ComponentAdded.Invoke(component);
		SceneModified.Invoke();
	}

	private void SetOpenGLState()
	{
		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);
		GL.Enable(EnableCap.StencilTest);


		GL.Enable(EnableCap.CullFace);
		GL.CullFace(CullFaceMode.Back);
		GL.FrontFace(FrontFaceDirection.Cw);
	}

	private void RestoreOpenGLState()
	{
		GL.Disable(EnableCap.CullFace);
	}

	public void RenderWorld()
	{
		SetOpenGLState();

		// GL.ClearDepth(1000);
		//
		//
		// GL.Viewport(0, 0, (int) Camera.I.Size.X, (int) Camera.I.Size.Y);
		// GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

		_sceneRenderQueue.RenderWorld();

		if (TransformHandle.Transform.IsInCanvas == false)
		{
			TransformHandle.I.GameObject.Render();
		}

		Tofu.I.InstancedRenderingSystem.RenderInstances();

		RestoreOpenGLState();
	}

	public void RenderUI()
	{
		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);
		GL.Enable(EnableCap.StencilTest);

		_sceneRenderQueue.RenderUI();

		if (TransformHandle.Transform.IsInCanvas)
		{
			TransformHandle.I.GameObject.Render();
		}
	}

	// public void RenderTransparent()
	// {
	// 	_sceneRenderQueue.RenderTransparent();
	// }

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

		_sceneRenderQueue.RenderQueueChanged();
	}

	public void AddGameObjectsToScene(IEnumerable<GameObject> gameObjects)
	{
		GameObjects.AddRange(gameObjects);

		_sceneRenderQueue.RenderQueueChanged();
	}

	public void CreateEmptySceneAndOpenIt(string path)
	{
		IDsManager.GameObjectNextId = 0;
		SceneManager.LastOpenedScene = path;
		GameObjects = new List<GameObject>();
		CreateDefaultObjects();
		SceneSerializer.SaveGameObjects(GetSceneFile(), path);
	}

	public void OnGameObjectDestroyed(GameObject gameObject)
	{
		if (GameObjects.Contains(gameObject))
		{
			GameObjects.Remove(gameObject);
		}

		SceneModified.Invoke();
	}

	void OnMouse3Clicked()
	{
	}

	void OnMouse3Released()
	{
	}
}