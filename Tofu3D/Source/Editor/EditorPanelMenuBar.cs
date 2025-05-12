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

    public override void Init()
    {
        I = this;
    }

    public override void Draw()
    {
        if (IsActive == false)
        {
            return;
        }

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
                bool settingsButtonClicked = ImGui.Button("Settings");
                if (settingsButtonClicked)
                {
                    Tofu.Editor.AfterDraw +=
                        () => EditorPanelEditorSettings.I.Toggle(true);
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

                bool saveSceneButtonClicked = ImGui.Button("Save scene");
                if (saveSceneButtonClicked)
                {
                    Tofu.Editor.AfterDraw +=
                        () => Tofu.SceneManager.SaveScene();
                    ImGui.CloseCurrentPopup();
                }

                bool realodSceneButtonClicked = ImGui.Button("Reload scene");
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
                    bool saveCurrentLayoutButtonClicked = ImGui.Button("Save Current Layout");
                    if (saveCurrentLayoutButtonClicked)
                    {
                        ImGui.CloseCurrentPopup();

                        _editorLayoutManager.SaveCurrentLayout();
                    }

                    bool loadDefaultLayoutButtonClicked = ImGui.Button("Load Default Layout");
                    if (loadDefaultLayoutButtonClicked)
                    {
                        ImGui.CloseCurrentPopup();

                        Tofu.Editor.BeforeDraw +=
                            _editorLayoutManager
                                .LoadDefaultLayout; // load layout before drawing anything, otherwise we break the layout by calling imgui after this editor panel
                    }

                    bool saveDefaultLayoutButtonClicked = ImGui.Button("Save Default Layout");
                    if (saveDefaultLayoutButtonClicked)
                    {
                        ImGui.CloseCurrentPopup();

                        _editorLayoutManager.SaveDefaultLayout();
                    }

                    ImGui.EndMenu();
                }


                ImGui.EndMenu();
            }


            bool skyboxButtonClicked = ImGui.BeginMenu("Skybox");
            if (skyboxButtonClicked)
            {
                EditorPanelInspector.I.SelectInspectable(Tofu.SceneManager.CurrentScene.FindComponent<Skybox>());

                ImGui.CloseCurrentPopup();


                ImGui.EndMenu();
            }

            bool instancedRenderingClicked = ImGui.BeginMenu("Instanced Rendering");
            if (instancedRenderingClicked)
            {
                EditorPanelInspector.I.SelectInspectable(Tofu.InstancedRenderingSystem);

                ImGui.CloseCurrentPopup();


                ImGui.EndMenu();
            }

            bool fpsLimiterButtonClicked = ImGui.BeginMenu($"FPS Limiter [{Tofu.Window.FrameLimiterEnabled}]");
            if (fpsLimiterButtonClicked)
            {
                Tofu.Window.FrameLimiterEnabled = !Tofu.Window.FrameLimiterEnabled;
                ImGui.CloseCurrentPopup();


                ImGui.EndMenu();
            }

            bool showDebugButton = true; // KeyboardInput.IsKeyDown(Keys.LeftAlt);
            if (showDebugButton)
            {
                bool debugButtonClicked = ImGui.SmallButton($"Debug [{(Global.Debug ? "ON" : "OFF")}]");
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