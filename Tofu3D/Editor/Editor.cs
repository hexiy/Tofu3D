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

	public Editor()
	{
		I = this;
	}

	public static Editor I { get; private set; }

	ImGuiWindowClassPtr _panelWindowClassPtr;

	public unsafe void Init()
	{
		EditorLayoutManager.LoadLastLayout();

		ImGuiWindowClass panelWindowClas = new ImGuiWindowClass() {DockNodeFlagsOverrideSet = ImGuiDockNodeFlags.None /*ImGuiDockNodeFlags.AutoHideTabBar*/};
		_panelWindowClassPtr = new ImGuiWindowClassPtr(&panelWindowClas);


		SetTheme();
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

	private void SetTheme()
	{
		ImGui.GetStyle().WindowRounding = 0;
		ImGui.GetStyle().WindowBorderSize = 0.2f;
		//ImGui.GetStyle().WindowPadding = new Vector2(0,0;

		ImGuiStylePtr style = ImGui.GetStyle();
		RangeAccessor<System.Numerics.Vector4> colors = style.Colors;

		int themeIndex = 3;
		if (themeIndex == 0)
		{
			colors[(int) ImGuiCol.Text] = new Vector4(1.000f, 1.000f, 1.000f, 1.000f);
			colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.500f, 0.500f, 0.500f, 1.000f);
			colors[(int) ImGuiCol.WindowBg] = new Vector4(0.180f, 0.180f, 0.180f, 1.000f);
			colors[(int) ImGuiCol.ChildBg] = new Vector4(0.280f, 0.280f, 0.280f, 0.000f);
			colors[(int) ImGuiCol.PopupBg] = new Vector4(0.313f, 0.313f, 0.313f, 1.000f);
			colors[(int) ImGuiCol.Border] = new Vector4(0.266f, 0.266f, 0.266f, 1.000f);
			colors[(int) ImGuiCol.BorderShadow] = new Vector4(0.000f, 0.000f, 0.000f, 0.000f);
			colors[(int) ImGuiCol.FrameBg] = new Vector4(0.160f, 0.160f, 0.160f, 1.000f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.200f, 0.200f, 0.200f, 1.000f);
			colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.280f, 0.280f, 0.280f, 1.000f);
			colors[(int) ImGuiCol.TitleBg] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.160f, 0.160f, 0.160f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.277f, 0.277f, 0.277f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.300f, 0.300f, 0.300f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.CheckMark] = new Vector4(1.000f, 1.000f, 1.000f, 1.000f);
			colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.391f, 0.391f, 0.391f, 1.000f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.Button] = new Vector4(1.000f, 1.000f, 1.000f, 0.000f);
			colors[(int) ImGuiCol.ButtonHovered] = new Vector4(1.000f, 1.000f, 1.000f, 0.156f);
			colors[(int) ImGuiCol.ButtonActive] = new Vector4(1.000f, 1.000f, 1.000f, 0.391f);
			colors[(int) ImGuiCol.Header] = new Vector4(0.313f, 0.313f, 0.313f, 1.000f);
			colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.391f, 0.391f, 0.391f, 1.000f);
			colors[(int) ImGuiCol.SeparatorActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.ResizeGrip] = new Vector4(1.000f, 1.000f, 1.000f, 0.250f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(1.000f, 1.000f, 1.000f, 0.670f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.Tab] = new Vector4(0.098f, 0.098f, 0.098f, 1.000f);
			colors[(int) ImGuiCol.TabHovered] = new Vector4(0.352f, 0.352f, 0.352f, 1.000f);
			colors[(int) ImGuiCol.TabActive] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.098f, 0.098f, 0.098f, 1.000f);
			colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.DockingPreview] = new Vector4(1.000f, 0.391f, 0.000f, 0.781f);
			colors[(int) ImGuiCol.DockingEmptyBg] = new Vector4(0.180f, 0.180f, 0.180f, 1.000f);
			colors[(int) ImGuiCol.PlotLines] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.586f, 0.586f, 0.586f, 1.000f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(1.000f, 1.000f, 1.000f, 0.156f);
			colors[(int) ImGuiCol.DragDropTarget] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavHighlight] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.000f, 0.000f, 0.000f, 0.586f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.000f, 0.000f, 0.000f, 0.586f);
			colors[(int) ImGuiCol.Separator] = colors[(int) ImGuiCol.Border];
		}

		if (themeIndex == 1)
		{
			colors[(int) ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
			colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
			colors[(int) ImGuiCol.WindowBg] = new Vector4(0.13f, 0.13f, 0.13f, 1.000f);
			colors[(int) ImGuiCol.ChildBg] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
			colors[(int) ImGuiCol.PopupBg] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
			colors[(int) ImGuiCol.Border] = new Vector4(0.25f, 0.25f, 0.25f, 1f);
			colors[(int) ImGuiCol.BorderShadow] = new Vector4(0.3f, 0.3f, 0.3f, 0f);
			colors[(int) ImGuiCol.FrameBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
			colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
			colors[(int) ImGuiCol.TitleBg] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
			colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
			colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
			colors[(int) ImGuiCol.CheckMark] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
			colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.08f, 0.50f, 0.72f, 1.00f);
			colors[(int) ImGuiCol.Button] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
			colors[(int) ImGuiCol.ButtonActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
			colors[(int) ImGuiCol.Header] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
			colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
			colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
			colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.41f, 0.42f, 0.44f, 1.00f);
			colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
			colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.29f, 0.30f, 0.31f, 0.67f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
			colors[(int) ImGuiCol.Tab] = new Vector4(0.08f, 0.08f, 0.09f, 0.83f);
			colors[(int) ImGuiCol.TabHovered] = new Vector4(0.33f, 0.34f, 0.36f, 0.83f);
			colors[(int) ImGuiCol.TabActive] = new Vector4(0.23f, 0.23f, 0.24f, 1.00f);
			colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
			colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
			colors[(int) ImGuiCol.DockingPreview] = new Vector4(0.26f, 0.59f, 0.98f, 0.70f);
			colors[(int) ImGuiCol.DockingEmptyBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
			colors[(int) ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
			colors[(int) ImGuiCol.DragDropTarget] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
			colors[(int) ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
			colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
			colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
			colors[(int) ImGuiCol.Separator] = colors[(int) ImGuiCol.Border];
		}

		if (themeIndex == 2)
		{
			colors[(int) ImGuiCol.Text] = new Vector4(0.00f, 0.00f, 0.00f, 0.85f);
			colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f);
			colors[(int) ImGuiCol.WindowBg] = new Vector4(0.94f, 0.94f, 0.94f, 1.00f);
			colors[(int) ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			colors[(int) ImGuiCol.PopupBg] = new Vector4(1.00f, 1.00f, 1.00f, 0.98f);
			colors[(int) ImGuiCol.Border] = new Vector4(0.00f, 0.00f, 0.00f, 0.44f);
			colors[(int) ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			colors[(int) ImGuiCol.FrameBg] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.64f, 0.65f, 0.66f, 0.40f);
			colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.64f, 0.65f, 0.66f, 0.40f);
			colors[(int) ImGuiCol.TitleBg] = new Vector4(0.82f, 0.82f, 0.82f, 1.00f);
			colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.71f, 0.70f, 0.70f, 1.00f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(1.00f, 1.00f, 1.00f, 0.51f);
			colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.86f, 0.86f, 0.86f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.98f, 0.98f, 0.98f, 0.53f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.69f, 0.69f, 0.69f, 0.80f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.49f, 0.49f, 0.49f, 0.80f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.49f, 0.49f, 0.49f, 1.00f);
			colors[(int) ImGuiCol.CheckMark] = new Vector4(0.43f, 0.43f, 0.43f, 1.00f);
			colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.63f, 0.63f, 0.63f, 0.78f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.43f, 0.44f, 0.46f, 0.78f);
			colors[(int) ImGuiCol.Button] = new Vector4(0.61f, 0.61f, 0.62f, 0.40f);
			colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.57f, 0.57f, 0.57f, 0.52f);
			colors[(int) ImGuiCol.ButtonActive] = new Vector4(0.61f, 0.63f, 0.64f, 1.00f);
			colors[(int) ImGuiCol.Header] = new Vector4(0.64f, 0.64f, 0.65f, 0.31f);
			colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.58f, 0.58f, 0.59f, 0.55f);
			colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.52f, 0.52f, 0.52f, 0.55f);
			colors[(int) ImGuiCol.Separator] = new Vector4(0.56f, 0.56f, 0.56f, 1.00f);
			colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.17f, 0.17f, 0.17f, 0.89f);
			colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.17f, 0.17f, 0.17f, 0.89f);
			colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.80f, 0.80f, 0.80f, 0.56f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.39f, 0.39f, 0.40f, 0.67f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.39f, 0.39f, 0.40f, 0.67f);
			// colors[(int)ImGuiCol.CloseButton]            = new Vector4(0.59f, 0.59f, 0.59f, 0.50f);
			// colors[(int)ImGuiCol.ButtonHovered]     = new Vector4(0.98f, 0.39f, 0.36f, 1.00f);
			// colors[(int)ImGuiCol.ButtonActive]      = new Vector4(0.98f, 0.39f, 0.36f, 1.00f);
			colors[(int) ImGuiCol.PlotLines] = new Vector4(0.39f, 0.39f, 0.39f, 1.00f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.78f, 0.78f, 0.78f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(0.56f, 0.56f, 0.56f, 1.00f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.71f, 0.72f, 0.73f, 0.57f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
			colors[(int) ImGuiCol.DragDropTarget] = new Vector4(0.16f, 0.16f, 0.17f, 0.95f);
		}

		if (themeIndex == 3) // custom one to look like my ide
		{
			Vector4 cTransparent = new(0, 0, 0, 0);
			Vector4 cBeige = new(1f, 0.96f, 0.90f, 1.00f);
			Vector4 cBeigeMid = new(0.97f, 0.94f, 0.88f, 1f);
			Vector4 cBeigeDarker = new(0.94f, 0.91f, 0.85f, 1f);
			Vector4 cScrollbar = new(0.74f, 0.71f, 0.65f, 0.8f);
			Vector4 cScrollbarDarker = new(0.64f, 0.61f, 0.55f, 1f);

			colors[(int) ImGuiCol.Text] = new Vector4(0.23f, 0.29f, 0.40f, 1f);
			colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.23f, 0.29f, 0.40f, 0.5f);
			colors[(int) ImGuiCol.WindowBg] = new Vector4(0.99f, 0.96f, 0.90f, 1.00f);
			colors[(int) ImGuiCol.ChildBg] = cBeige;
			colors[(int) ImGuiCol.PopupBg] = cBeige;
			colors[(int) ImGuiCol.Border] = new Vector4(0.81f, 0.79f, 0.74f, 1);
			colors[(int) ImGuiCol.BorderShadow] = cTransparent;
			colors[(int) ImGuiCol.FrameBg] = cBeige;
			colors[(int) ImGuiCol.FrameBgHovered] = cBeige;
			colors[(int) ImGuiCol.FrameBgActive] = cBeige;
			colors[(int) ImGuiCol.TitleBg] = cBeige;
			colors[(int) ImGuiCol.TitleBgActive] = cBeige;
			colors[(int) ImGuiCol.TitleBgCollapsed] = cBeige;
			colors[(int) ImGuiCol.MenuBarBg] = cBeige;
			colors[(int) ImGuiCol.ScrollbarBg] = cBeige;
			colors[(int) ImGuiCol.ScrollbarGrab] = cScrollbar;
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = cScrollbarDarker;
			colors[(int) ImGuiCol.ScrollbarGrabActive] = cScrollbarDarker;
			colors[(int) ImGuiCol.CheckMark] = new Vector4(0.43f, 0.43f, 0.43f, 1.00f);
			colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.63f, 0.63f, 0.63f, 0.78f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.43f, 0.44f, 0.46f, 0.78f);
			colors[(int) ImGuiCol.Button] = cBeigeDarker;
			colors[(int) ImGuiCol.ButtonHovered] = cBeigeMid;
			colors[(int) ImGuiCol.ButtonActive] = cBeigeDarker;
			colors[(int) ImGuiCol.Header] = cBeigeDarker;
			colors[(int) ImGuiCol.HeaderHovered] = cBeigeMid;
			colors[(int) ImGuiCol.HeaderActive] = cBeigeDarker;
			colors[(int) ImGuiCol.Separator] = new Vector4(0.81f, 0.79f, 0.74f, 1);
			colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.17f, 0.17f, 0.17f, 0.89f);
			colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.17f, 0.17f, 0.17f, 0.89f);
			colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.80f, 0.80f, 0.80f, 0.56f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.39f, 0.39f, 0.40f, 0.67f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.39f, 0.39f, 0.40f, 0.67f);
			colors[(int) ImGuiCol.Tab] = cBeige;
			colors[(int) ImGuiCol.TabActive] = cBeige;
			colors[(int) ImGuiCol.TabUnfocused] = cBeige;
			colors[(int) ImGuiCol.TabUnfocusedActive] = cBeige;
			colors[(int) ImGuiCol.TabHovered] = cBeige;
			// colors[(int)ImGuiCol.CloseButton]            = new Vector4(0.59f, 0.59f, 0.59f, 0.50f);
			// colors[(int)ImGuiCol.ButtonHovered]     = new Vector4(0.98f, 0.39f, 0.36f, 1.00f);
			// colors[(int)ImGuiCol.ButtonActive]      = new Vector4(0.98f, 0.39f, 0.36f, 1.00f);
			colors[(int) ImGuiCol.PlotLines] = new Vector4(0.39f, 0.39f, 0.39f, 1.00f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.78f, 0.78f, 0.78f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(0.56f, 0.56f, 0.56f, 1.00f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.71f, 0.72f, 0.73f, 0.57f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
			colors[(int) ImGuiCol.DragDropTarget] = new Vector4(0.16f, 0.16f, 0.17f, 0.95f);
		}


		// style.WindowPadding = new Vector2(15, 15);
		// style.WindowRounding = 5.0f;
		// style.FramePadding = new System.Numerics.Vector2(5, 5);
		// style.FrameRounding = 4.0f;
		// style.ItemSpacing = new Vector2(12, 8);
		// style.ItemInnerSpacing = new Vector2(8, 6);
		// style.IndentSpacing = 25.0f;
		// style.ScrollbarSize = 15.0f;
		// style.ScrollbarRounding = 9.0f;
		// style.GrabMinSize = 5.0f;
		// style.GrabRounding = 3.0f;
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
				Scene.I.SaveScene();
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftSuper) && KeyboardInput.WasKeyJustPressed(Keys.R))
		{
			if (Global.GameRunning == false)
			{
				Scene.I.LoadScene(Serializer.LastScene);
			}
		}
	}

	public void Draw()
	{
		BeforeDraw.Invoke();
		BeforeDraw = () => { };
		// ImGui.DockSpace(0, new System.Numerics.Vector2(2300, 2300), ImGuiDockNodeFlags.NoDockingInCentralNode);
		ImGuiViewportPtr viewportPtr = ImGui.GetWindowViewport();

		ImGui.DockSpaceOverViewport(viewportPtr, ImGuiDockNodeFlags.PassthruCentralNode /*, ImGuiDockNodeFlags.NoDockingInCentralNode*/);
		// ImGui.DockSpace(1, new System.Numerics.Vector2(500, 500));

		// ImGui.PopStyleVar(3);
		// ImGui.DockSpace(0, ImGui.GetWindowSize());
		// ImGui.SetCursorPos(new Vector2(0, 0));

		if (Global.EditorAttached)
		{
			for (int i = 0; i < _editorPanels.Length; i++)
			{
				// ImGui.SetNextWindowClass(_panelWindowClassPtr);
				_editorPanels[i].Draw();
			}

			//EditorWindow_Console.I.Draw();
		}
		else
		{
			EditorPanelSceneView.I.Draw();
		}

		// ImGui.SetNextWindowSize(new Vector2(Window.I.ClientSize.X, Window.I.ClientSize.Y), ImGuiCond.Always);
		// ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero, ImGuiCond.Always, new Vector2(0, 0));

		// ImGui.Begin("Docking space", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar);
		//
		// ImGui.End();
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