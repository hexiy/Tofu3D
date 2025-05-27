using ImGuiNET;

namespace TofuEngine;

public class EditorPanelMenuBar : EditorPanel
{
    private readonly EditorLayoutManager _editorLayoutManager;


    public EditorPanelMenuBar(EditorLayoutManager editorLayoutManager)
    {
        _editorLayoutManager = editorLayoutManager;
    }

    public static EditorPanelMenuBar I { get; private set; }
    public override string Name => "Menu Bar";
    public override bool CreatesWindow => false;

    public override void Init()
    {
        I = this;
    }

    protected override void ExecuteImGuiDrawCommands()
    {
        if (Global.EditorAttached)
        {
            ImGui.SetNextWindowSize(new Vector2(Tofu.Window.Size.X * 2, 50), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.FirstUseEver, new Vector2(0, 0));
            // ImGui.PushStyleColor(ImGuiCol.WindowBg, Color.Red.ToVector4());
            // ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar);
            ImGui.BeginMainMenuBar();

            using TofuImGuiSetItemSpacingGuard itemSpacingGuard =
                new TofuImGuiSetItemSpacingGuard(newSpacingX: TofuImGui.DefaultItemSpacing.X + 8);
            // using TofuImGuiSetItemSpacingGuard itemSpacingGuard =
                // TofuImGui.SetTemporaryItemSpacingForCurrentScope(x: TofuImGui.DefaultItemSpacing.X + 5);

            bool tofu3dMenuOpened = ImGui.BeginMenu("Tofu3D");
            if (tofu3dMenuOpened)
            {
                bool settingsButtonClicked = ImGui.MenuItem("Settings");
                if (settingsButtonClicked)
                {
                    Tofu.Editor.AfterDraw +=
                        () =>
                        {
                            Tofu.Editor.OpenWindow( new EditorPanelEditorSettings());
                        };
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndMenu();
            }


            bool fileMenuOpened = ImGui.BeginMenu("File");
            if (fileMenuOpened)
            {
                // bool newSceneButtonClicked = ImGui.Button("New scene");
                // if (newSceneButtonClicked)
                // {
                //     Tofu.Editor.AfterDraw +=
                //         () => Tofu.SceneManager.LoadLastOpenedScene();
                //     ImGui.CloseCurrentPopup();
                // }

                bool saveSceneButtonClicked = ImGui.MenuItem("Save scene");
                if (saveSceneButtonClicked)
                {
                    Tofu.Editor.AfterDraw +=
                        () => Tofu.SceneManager.SaveScene();
                    ImGui.CloseCurrentPopup();
                }

                bool realodSceneButtonClicked = ImGui.MenuItem("Reload scene");
                if (realodSceneButtonClicked)
                {
                    Tofu.Editor.AfterDraw +=
                        () => Tofu.SceneManager.ReloadScene();
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndMenu();
            }

            bool editMenuOpened = ImGui.BeginMenu("Edit");
            if (editMenuOpened)
            {
                ImGui.EndMenu();
            }


            bool windowMenuClicked = ImGui.BeginMenu("Window");
            if (windowMenuClicked)
            {
                bool layoutMenuOpened = ImGui.BeginMenu("Layout");
                if (layoutMenuOpened)
                {
                    bool saveCurrentLayoutButtonClicked = ImGui.MenuItem("Save Current Layout");
                    if (saveCurrentLayoutButtonClicked)
                    {
                        ImGui.CloseCurrentPopup();

                        _editorLayoutManager.SaveCurrentLayout();
                    }

                    bool loadDefaultLayoutButtonClicked = ImGui.MenuItem("Load Default Layout");
                    if (loadDefaultLayoutButtonClicked)
                    {
                        ImGui.CloseCurrentPopup();

                        Tofu.Editor.BeforeDraw +=
                            _editorLayoutManager
                                .LoadDefaultLayout; // load layout before drawing anything, otherwise we break the layout by calling imgui after this editor panel
                    }

                    bool saveDefaultLayoutButtonClicked = ImGui.MenuItem("Save Default Layout");
                    if (saveDefaultLayoutButtonClicked)
                    {
                        ImGui.CloseCurrentPopup();

                        _editorLayoutManager.SaveDefaultLayout();
                    }

                    ImGui.EndMenu();
                }


                ImGui.EndMenu();
            }


            bool skyboxButtonClicked = ImGui.MenuItem("Skybox");
            if (skyboxButtonClicked)
            {
                EditorPanelInspector.I.SelectInspectable(Tofu.SceneManager.CurrentScene.FindComponent<Skybox>());
            }

            bool instancedRenderingClicked = ImGui.MenuItem("Instanced Rendering");
            if (instancedRenderingClicked)
            {
                EditorPanelInspector.I.SelectInspectable(Tofu.InstancedRenderingSystem);
            }

            bool fpsLimiterButtonClicked = ImGui.MenuItem($"FPS Limiter [{Tofu.Window.FrameLimiterEnabled}]");
            if (fpsLimiterButtonClicked)
            {
                Tofu.Window.FrameLimiterEnabled = !Tofu.Window.FrameLimiterEnabled;
            }

            bool showDebugButton = true; // KeyboardInput.IsKeyDown(Keys.LeftAlt);
            if (showDebugButton)
            {
                bool debugButtonClicked = ImGui.MenuItem($"Debug [{(Global.Debug ? "ON" : "OFF")}]");
                if (debugButtonClicked)
                {
                    Global.Debug = !Global.Debug;
                }
            }

            ImGui.EndMainMenuBar();

            // ImGui.PopStyleColor();
        }
    }

    public override void Update()
    {
    }
}