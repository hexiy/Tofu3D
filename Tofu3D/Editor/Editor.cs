using ImGuiNET;

namespace Tofu3D;

public class Editor
{
	public Vector2 SceneViewPosition = new(0, 0);
	public Vector2 SceneViewSize = new(0, 0);
	//private ImGuiRenderer _imGuiRenderer;
	EditorPanel[] _editorPanels;
	private EditorDialogManager _editorDialogManager;

	List<RangeAccessor<System.Numerics.Vector4>> _themes = new();

	// Is cleared after invocation
	public Action BeforeDraw = () => { };

	//public static Vector2 ScreenToWorld(Vector2 screenPosition)
	//{
	//	return (screenPosition - gameViewPosition) * Camera.I.cameraSize + Camera.I.transform.position;
	//}
	public static readonly ImGuiWindowFlags ImGuiDefaultWindowFlags = ImGuiWindowFlags.NoCollapse /* | ImGuiWindowFlags.AlwaysAutoResize*/ /* | ImGuiWindowFlags.NoDocking*/;

	ImGuiWindowClassPtr _panelWindowClassPtr;
	public EditorTextures EditorTextures;
	public unsafe void Initialize()
	{
		EditorLayoutManager.LoadLastLayout();
		EditorThemeing.SetTheme();

		EditorTextures = new EditorTextures();

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

		_editorDialogManager = new EditorDialogManager();
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
		_editorDialogManager.Update();

		if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustPressed(Keys.S))
		{
			if (Global.GameRunning == false)
			{
				Tofu.I.SceneManager.SaveScene();
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustPressed(Keys.R))
		{
			if (Global.GameRunning == false)
			{
				Tofu.I.SceneManager.LoadLastOpenedScene();
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.Escape))
		{
			ShowDialog(new EditorDialogParams("yo.", 
				new EditorDialogButtonDefinition("close", clicked:Tofu.I.Window.Close, closeOnClick:true),
				new EditorDialogButtonDefinition("no", clicked: () => { }, closeOnClick:true)));
			// Tofu.I.Window.Close();
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
		
		_editorDialogManager.Draw();
	}

	public EditorDialogHandle ShowDialog(EditorDialogParams editorDialogParams)
	{
		return _editorDialogManager.ShowDialog(editorDialogParams);
	}

	public void HideDialog(EditorDialogHandle dialogHandle)
	{
		_editorDialogManager.HideDialog(dialogHandle);
	}
}