using ImGuiNET;

namespace Tofu3D;

public class Editor
{
    //public static Vector2 ScreenToWorld(Vector2 screenPosition)
    //{
    //	return (screenPosition - gameViewPosition) * Camera.I.cameraSize + Camera.I.transform.position;
    //}
    public static readonly ImGuiWindowFlags
        ImGuiDefaultWindowFlags = ImGuiWindowFlags.NoCollapse /* | ImGuiWindowFlags.AlwaysAutoResize*/
        /* | ImGuiWindowFlags.NoDocking*/;

    private EditorDialogManager _editorDialogManager;

    private EditorLayoutManager _editorLayoutManager;

    //private ImGuiRenderer _imGuiRenderer;
    private EditorPanel[] _editorPanels;
    private EditorDialogHandle _exitDialogHandle;

    private ImGuiWindowClassPtr _panelWindowClassPtr;
    private List<RangeAccessor<System.Numerics.Vector4>> _themes = new();

    // Is cleared after invocation
    public Action BeforeDraw = () => { };

    public EditorTextures EditorTextures;

    // Left Bottom corner of the scene view
    public Vector2 SceneViewPosition = new(0, 0);

    public Vector2 SceneViewSize = new(0, 0);

    public unsafe void Initialize()
    {
        _editorLayoutManager = new EditorLayoutManager();
        _editorLayoutManager.LoadLastLayout();
        EditorThemeing.SetTheme();

        EditorTextures = new EditorTextures();

        ImGuiWindowClass panelWindowClas = new()
            { DockNodeFlagsOverrideSet = ImGuiDockNodeFlags.None /*ImGuiDockNodeFlags.AutoHideTabBar*/ };
        _panelWindowClassPtr = new ImGuiWindowClassPtr(&panelWindowClas);

        if (Global.EditorAttached)
        {
            _editorPanels = new EditorPanel[]
            {
                new EditorPanelMenuBar(_editorLayoutManager),
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

        for (var i = 0; i < _editorPanels.Length; i++)
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
        _editorLayoutManager.Update();

        for (var i = 0; i < _editorPanels.Length; i++)
        {
            _editorPanels[i].Update();
        }

        _editorDialogManager.Update();

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustPressed(Keys.S))
        {
            if (Global.GameRunning == false)
            {
                Tofu.SceneManager.SaveScene();
            }
        }

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustPressed(Keys.R))
        {
            if (Global.GameRunning == false)
            {
                Tofu.SceneManager.LoadLastOpenedScene();
            }
        }

        var exitDialogIsActive = _editorDialogManager.IsDialogActive(_exitDialogHandle);
        if (KeyboardInput.WasKeyJustPressed(Keys.Escape))
        {
            if (exitDialogIsActive)
            {
                Tofu.Window.Close();
                return;
            }

            _exitDialogHandle = ShowDialog(new EditorDialogParams("Close Tofu3D?",
                new EditorDialogButtonDefinition("Close", Tofu.Window.Close, true),
                new EditorDialogButtonDefinition("No", () => { }, true)));
        }
    }

    public void Draw()
    {
        BeforeDraw.Invoke();
        BeforeDraw = () => { };
        var viewportPtr = ImGui.GetWindowViewport();

        ImGui.DockSpaceOverViewport(viewportPtr,
            ImGuiDockNodeFlags.PassthruCentralNode /*, ImGuiDockNodeFlags.NoDockingInCentralNode*/);


        if (Global.EditorAttached)
        {
            for (var i = 0; i < _editorPanels.Length; i++)
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

    public EditorDialogHandle ShowDialog(EditorDialogParams editorDialogParams) =>
        _editorDialogManager.ShowDialog(editorDialogParams);

    public void HideDialog(EditorDialogHandle dialogHandle)
    {
        _editorDialogManager.HideDialog(dialogHandle);
    }
}