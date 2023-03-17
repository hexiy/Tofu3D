using ImGuiNET;

namespace Tofu3D;

public class Editor
{
	public static Vector2 SceneViewPosition = new(0, 0);
	public static Vector2 SceneViewSize = new(0, 0);
	//private ImGuiRenderer _imGuiRenderer;
	EditorPanel[] _editorPanels;

	List<RangeAccessor<System.Numerics.Vector4>> _themes = new();

	public TransformHandle TransformHandle;

	// Is cleared after invocation
	public static Action BeforeDraw = () => { };

	//public static Vector2 ScreenToWorld(Vector2 screenPosition)
	//{
	//	return (screenPosition - gameViewPosition) * Camera.I.cameraSize + Camera.I.transform.position;
	//}
	public static readonly ImGuiWindowFlags ImGuiDefaultWindowFlags = ImGuiWindowFlags.NoCollapse /* | ImGuiWindowFlags.AlwaysAutoResize*/ /* | ImGuiWindowFlags.NoDocking*/;

	ImGuiWindowClassPtr _panelWindowClassPtr;

	public unsafe void Init()
	{
		EditorLayoutManager.LoadLastLayout();
		EditorThemeing.SetTheme();

		ImGuiWindowClass panelWindowClas = new ImGuiWindowClass() {DockNodeFlagsOverrideSet = ImGuiDockNodeFlags.None /*ImGuiDockNodeFlags.AutoHideTabBar*/};
		_panelWindowClassPtr = new ImGuiWindowClassPtr(&panelWindowClas);

		if (Global.EditorAttached)
		{
			_editorPanels = new EditorPanel[]
			                {
				                new EditorPanelMenuBar(),
				                new EditorPanelHierarchy(),
				                new EditorPanelInspector(),
				                new EditorPanelBrowser(),
				                new EditorPanelConsole(),
				                new EditorPanelProfiler(),
				                new EditorPanelSceneView()
			                };
		}

		else
		{
			_editorPanels = new EditorPanel[]
			                {
				                new EditorPanelSceneView()
			                };
		}

		for (int i = 0; i < _editorPanels.Length; i++)
		{
			_editorPanels[i].Init();
		}

		if (Global.EditorAttached)
		{
			EditorPanelHierarchy.I.GameObjectsSelected += OnGameObjectSelected;
			EditorPanelHierarchy.I.GameObjectsSelected += EditorPanelInspector.I.OnGameObjectsSelected;
		}
	}

	public void Update()
	{
		EditorLayoutManager.Update();
		for (int i = 0; i < _editorPanels.Length; i++)
		{
			_editorPanels[i].Update();
		}

		/*if (KeyboardInput.IsKeyDown(Keys.LeftSuper) && KeyboardInput.WasKeyJustPressed(Keys.S))
		{
			if (Global.GameRunning == false)
			{
				Scene.I.SaveScene();
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftSuper) && KeyboardInput.WasKeyJustPressed(Keys.R))
		{
			if (Global.GameRunning == false)
			{
				Scene.I.LoadScene(SceneSerializer.LastScene);
			}
		}*/
	}

	public void Draw()
	{
		BeforeDraw.Invoke();
		BeforeDraw = () => { };
		ImGuiViewportPtr viewportPtr = ImGui.GetWindowViewport();

		ImGui.DockSpaceOverViewport(viewportPtr, ImGuiDockNodeFlags.PassthruCentralNode /*, ImGuiDockNodeFlags.NoDockingInCentralNode*/);


		if (Global.EditorAttached)
		{
			for (int i = 0; i < _editorPanels.Length; i++)
			{
				_editorPanels[i].Draw();
			}
		}
		else
		{
			EditorPanelSceneView.I.Draw();
		}
	}

	public void SelectGameObjects(List<int> goIds)
	{
		if (goIds != null)
		{
			for (int i = 0; i < Scene.I.GameObjects.Count; i++)
			{
				if (goIds.Contains(Scene.I.GameObjects[i].Id) == false)
				{
					Scene.I.GameObjects[i].Selected = false;
				}
			}

			for (int i = 0; i < goIds.Count; i++)
			{
				GameObject go = Scene.I.GetGameObject(goIds[i]);
				if (go != null)
				{
					go.Selected = true;
				}
			}
		}

		bool isCameraOrTransformHandle = false;
		if (Camera.I != null)
		{
			isCameraOrTransformHandle = goIds.Contains(Camera.I.GameObjectId) || goIds.Contains(TransformHandle.GameObjectId);
		}

		if (isCameraOrTransformHandle == false && goIds.Count != 0)
		{
			TransformHandle.SelectObjects(goIds);
			PersistentData.Set("lastSelectedGameObjectId", goIds[0]);
		}
		else
		{
			TransformHandle.SelectObjects(null);
		}
	}

	void OnGameObjectSelected(List<int> ids)
	{
		if (Global.EditorAttached == false)
		{
			ids = null;
		}

		if (ids == null)
		{
			SelectGameObjects(null);
		}
		else
		{
			// if (GetGameObjectIndexInHierarchy(ids) == -1)
			// {
			// 	return;
			// }

			SelectGameObjects(ids);
		}
	}

	public int GetGameObjectIndexInHierarchy(int id)
	{
		for (int i = 0; i < Scene.I.GameObjects.Count; i++)
		{
			if (Scene.I.GameObjects[i].Id == id)
			{
				return i;
			}
		}

		return -1;
	}

	public List<GameObject> GetSelectedGameObjects()
	{
		List<GameObject> selectedGameObjects = new();
		for (int i = 0; i < Scene.I.GameObjects.Count; i++)
		{
			if (Scene.I.GameObjects[i].Selected)
			{
				selectedGameObjects.Add(Scene.I.GameObjects[i]);
			}
		}

		return selectedGameObjects;
	}

	public GameObject GetSelectedGameObject()
	{
		for (int i = 0; i < Scene.I.GameObjects.Count; i++)
		{
			if (Scene.I.GameObjects[i].Selected)
			{
				return Scene.I.GameObjects[i];
			}
		}

		return null;
	}
}