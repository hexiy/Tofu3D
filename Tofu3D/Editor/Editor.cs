using ImGuiNET;

namespace Tofu3D;

public class Editor
{
	public static Vector2 SceneViewPosition = new(0, 0);
	public static Vector2 SceneViewSize = new(0, 0);
	//private ImGuiRenderer _imGuiRenderer;
	EditorPanel[] _editorPanels;

	List<RangeAccessor<System.Numerics.Vector4>> _themes = new();

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
			// EditorPanelHierarchy.I.GameObjectsSelected += OnGameObjectSelected;
			// EditorPanelHierarchy.I.GameObjectsSelected += EditorPanelInspector.I.OnGameObjectsSelected;
		}
	}

	public void Update()
	{
		EditorLayoutManager.Update();
		for (int i = 0; i < _editorPanels.Length; i++)
		{
			_editorPanels[i].Update();
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftSuper) && KeyboardInput.WasKeyJustPressed(Keys.S))
		{
			if (Global.GameRunning == false)
			{
				SceneManager.SaveScene();
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftSuper) && KeyboardInput.WasKeyJustPressed(Keys.R))
		{
			if (Global.GameRunning == false)
			{
				SceneManager.LoadLastOpenedScene();
			}
		}
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

	
}