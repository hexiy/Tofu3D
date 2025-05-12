using ImGuiNET;

namespace TofuEngine;

public class Editor
{
    //public static Vector2 ScreenToWorld(Vector2 screenPosition)
    //{
    //	return (screenPosition - gameViewPosition) * Camera.I.cameraSize + Camera.I.transform.position;
    //}
    private bool _sceneViewFullscreen = false;

    public static readonly ImGuiWindowFlags
        ImGuiDefaultWindowFlags =
            ImGuiWindowFlags.NoCollapse// | ImGuiWindowFlags.NoMove /* | ImGuiWindowFlags.AlwaysAutoResize*/
        /* | ImGuiWindowFlags.NoDocking*/;

    private EditorDialogManager _editorDialogManager;

    private EditorLayoutManager _editorLayoutManager;

    //private ImGuiRenderer _imGuiRenderer;
    private EditorPanel[] _editorPanels;
    private EditorDialogHandle _exitDialogHandle;

    private ImGuiWindowClassPtr _panelWindowClassPtr;
    private List<RangeAccessor<System.Numerics.Vector4>> _themes = new List<RangeAccessor<System.Numerics.Vector4>>();

    // Is cleared after invocation
    public Action BeforeDraw = () => { };
    public Action AfterDraw = () => { };

    public EditorTextures EditorTextures;

    // Left Bottom corner of the scene view
    public Vector2 SceneViewPosition = new Vector2(0, 0);

    public Vector2 SceneViewSize = new Vector2(0, 0);

    
    // Left Bottom corner of the scene view
    public Vector2 GameViewPosition = new Vector2(0, 0);

    public Vector2 GameViewSize = new Vector2(0, 0);
    public unsafe void Initialize()
    {
        _editorLayoutManager = new EditorLayoutManager();
        _editorLayoutManager.LoadLastLayout();
        EditorThemeing.SetTheme(Tofu.EditorSettingsAll.EditorSettingsGeneral.EditorTheme);

        EditorTextures = new EditorTextures();

        ImGuiWindowClass panelWindowClas = new ImGuiWindowClass
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
                new EditorPanelSceneView(),
                new EditorPanelGameView(),
                new EditorPanelEditorSettings(),
                // new EditorPanelTextureViewer(),
            };
        }
        else
        {
            _editorPanels = new EditorPanel[]
            {
                new EditorPanelMenuBar(_editorLayoutManager),
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
        _editorLayoutManager.Update();

        for (int i = 0; i < _editorPanels.Length; i++)
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
                Tofu.SceneManager.ReloadScene();
            }
        }

        if (Tofu.EditorWindowsManager.AnyWindowOpen == false)
        {
            bool exitDialogIsActive = _editorDialogManager.IsDialogActive(_exitDialogHandle);
            if (KeyboardInput.WasKeyJustPressed(Keys.Escape))
            {
                if (exitDialogIsActive)
                {
                    // Tofu.Window.Close();
                    _editorDialogManager.HideDialog(_exitDialogHandle);
                    return;
                }


                _exitDialogHandle = ShowDialog(new EditorDialogParams("Close Tofu3D?",
                    new EditorDialogButtonDefinition("Close", Tofu.Window.Close, true),
                    new EditorDialogButtonDefinition("No", () => { }, true)));
            }
        }

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustPressed(Keys.F))
        {
            ToggleFullscreenOfSceneView();
        }
    }

    private void ToggleFullscreenOfSceneView()
    {
        _sceneViewFullscreen = !_sceneViewFullscreen;
    }

    public void Draw()
    {
        BeforeDraw.Invoke();
        BeforeDraw = () => { };
        ImGuiViewportPtr viewportPtr = ImGui.GetWindowViewport();

        ImGui.DockSpaceOverViewport(viewportPtr,
            ImGuiDockNodeFlags.PassthruCentralNode /*, ImGuiDockNodeFlags.NoDockingInCentralNode*/);

        if (_sceneViewFullscreen || Global.EditorAttached == false)
        {
            EditorPanelMenuBar.I.Draw();

            EditorPanelSceneView.I.IsFullscreen = true;
            EditorPanelSceneView.I.Draw();
        }
        else
        {
            for (int i = 0; i < _editorPanels.Length; i++)
            {
                _editorPanels[i].Draw();
            }
        }

        _editorDialogManager.Draw();

        AfterDraw.Invoke();
        AfterDraw = () => { };
    }

    public EditorDialogHandle ShowDialog(EditorDialogParams editorDialogParams) =>
        _editorDialogManager.ShowDialog(editorDialogParams);

    public void HideDialog(EditorDialogHandle dialogHandle)
    {
        _editorDialogManager.HideDialog(dialogHandle);
    }
}