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
            ImGuiWindowFlags.NoCollapse // | ImGuiWindowFlags.NoMove /* | ImGuiWindowFlags.AlwaysAutoResize*/
        /* | ImGuiWindowFlags.NoDocking*/;

    private EditorDialogManager _editorDialogManager;

    private EditorLayoutManager _editorLayoutManager;

    //private ImGuiRenderer _imGuiRenderer;
    private List<EditorPanel> _editorPanels;
    private EditorDialogHandle _exitDialogHandle;

    private ImGuiWindowClassPtr _panelWindowClassPtr;
    private List<RangeAccessor<System.Numerics.Vector4>> _themes = new List<RangeAccessor<System.Numerics.Vector4>>();

    // Is cleared after invocation
    public Action BeforeDraw = () => { };
    public Action AfterDraw = () => { };

    public EditorTextures EditorTextures;

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
            _editorPanels = new List<EditorPanel>
            {
                new EditorPanelMenuBar(_editorLayoutManager),
                new EditorPanelToolbar(),
                new EditorPanelHierarchy(),
                new EditorPanelInspector(),
                new EditorPanelBrowser(),
                new EditorPanelConsole(),
                new EditorPanelProfiler(),
                new EditorPanelSceneView(),
                // new EditorPanelSceneView(),
                new EditorPanelGameView(),
                new EditorPanelTextureViewer(),
            };
        }
        else
        {
            _editorPanels = new List<EditorPanel>
            {
                new EditorPanelGameView()
            };
        }

        for (int i = 0; i < _editorPanels.Count; i++)
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

        for (int i = 0; i < _editorPanels.Count; i++)
        {
            _editorPanels[i].Update();
        }

        _editorDialogManager.Update();

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustPressed(Keys.S))
        {
            if (Playmode.GameRunning == false)
            {
                Tofu.SceneManager.SaveScene();
            }
        }

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustPressed(Keys.R))
        {
            if (Playmode.GameRunning == false)
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
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();

        System.Numerics.Vector2 dockspacePos =
            viewport.WorkPos + new System.Numerics.Vector2(0, EditorPanelToolbar.Height);
        System.Numerics.Vector2
            dockspaceSize = viewport.WorkSize - new System.Numerics.Vector2(0, EditorPanelToolbar.Height);

        ImGui.SetNextWindowPos(dockspacePos);
        ImGui.SetNextWindowSize(dockspaceSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiWindowFlags hostWindowFlags = ImGuiWindowFlags.NoDocking |
                                           ImGuiWindowFlags.NoTitleBar |
                                           ImGuiWindowFlags.NoCollapse |
                                           ImGuiWindowFlags.NoResize |
                                           ImGuiWindowFlags.NoMove |
                                           ImGuiWindowFlags.NoBringToFrontOnFocus |
                                           ImGuiWindowFlags.NoNavFocus |
                                           ImGuiWindowFlags.NoBackground;

        TofuImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
        TofuImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        TofuImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);

        if (Global.EditorAttached == false)
        {
            Debug.Log("todo fullscreen");
            // EditorPanelGameView.I.IsFullscreen = true;
            // EditorPanelGameView.I.Draw();
        }
        else
        {
            if (ImGui.Begin("DockSpaceHostWindow", hostWindowFlags))
            {
                TofuImGui.PopStyleVar(3);

                uint dockspaceId = ImGui.GetID("MyDockSpace");
                ImGui.DockSpace(dockspaceId, System.Numerics.Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

                // if (Global.EditorAttached == false)
                // {
                //     EditorPanelGameView.I.IsFullscreen = true;
                //     EditorPanelGameView.I.Draw();
                // }

                /*else*/
                if (_sceneViewFullscreen)
                {
                    EditorPanelMenuBar.I.Draw();

                    EditorViewManager.LastUsedView.IsFullscreen = true;
                    EditorViewManager.LastUsedView.Draw();
                }
                else
                {
                    for (int i = 0; i < _editorPanels.Count; i++)
                    {
                        _editorPanels[i].Draw();
                    }
                }
            }
            else
            {
                TofuImGui.PopStyleVar(3);
            }
        }

        ImGui.End();

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

    public void OpenWindow(EditorPanel editorPanel)
    {
        editorPanel.Init();
        _editorPanels.Add(editorPanel);
    }

    public void CloseWindow(EditorPanel editorPanel)
    {
        _editorPanels.Remove(editorPanel);
    }
}