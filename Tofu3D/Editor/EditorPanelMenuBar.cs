using ImGuiNET;

namespace Tofu3D;

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
        if (Active == false)
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

            bool layoutButtonClicked = ImGui.BeginMenu("Layout");
            if (layoutButtonClicked)
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

            bool persistentDataButtonClicked = ImGui.BeginMenu("Persistent Data");
            if (persistentDataButtonClicked)
            {
                bool resetPersistentDataButtonClicked = ImGui.Button("Reset");
                if (resetPersistentDataButtonClicked)
                {
                    ImGui.CloseCurrentPopup();

                    PersistentData.DeleteAll();
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

            bool editorSettingsClicked = ImGui.BeginMenu($"Editor Settings");
            if (editorSettingsClicked)
            {
                Tofu.Editor.AfterDraw +=
                    () => EditorPanelEditorSettings.I.Toggle(!EditorPanelEditorSettings.I.Active);

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