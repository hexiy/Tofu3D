using OpenTK.Windowing.Common;
using Tofu3D.Rendering;
using Tofu3D.Tweening;

namespace Tofu3D;

// Main Application Context
public static class Tofu
{
    // public static Tofu I { get; private set; }

    // EDITOR
    public static Window Window;
    public static Editor Editor;
    public static ImGuiController ImGuiController;

    // RENDERING
    public static RenderSettings RenderSettings;
    public static RenderPassSystem RenderPassSystem;
    public static ShaderManager ShaderManager;
    public static InstancedRenderingSystem InstancedRenderingSystem;

    // ASSETS
    public static AssetManager AssetManager;
    public static SceneSerializer SceneSerializer;
    public static AssetsWatcher AssetsWatcher;

    // SCENE
    public static SceneManager SceneManager;
    public static SceneViewController SceneViewController;

    // MISC
    public static TweenManager TweenManager;

    // INPUT
    public static MouseInput MouseInput;

    static Tofu()
    {
        // I = this;
    }

    public static void Launch()
    {
        SystemConfig.Configure();
        Global.LoadSavedData();

        AssetManager = new AssetManager();
        SceneManager = new SceneManager();
        SceneSerializer = new SceneSerializer();
        RenderSettings = new RenderSettings();
        AssetsWatcher = new AssetsWatcher();
        ShaderManager = new ShaderManager();
        TweenManager = new TweenManager();
        MouseInput = new MouseInput();

        RenderSettings.LoadSavedData();
        AssetsWatcher.StartWatching();
        ShaderManager.Initialize();

        Window = new Window();
        Window.Load += OnWindowLoad;
        Window.UpdateFrame += OnWindowUpdate;
        Window.RenderFrame += OnWindowRender;
        Window.Run();
    }

    private static void OnWindowLoad()
    {
        InstancedRenderingSystem = new InstancedRenderingSystem();

        RenderPassSystem = new RenderPassSystem();
        RenderPassSystem.Initialize();

        ImGuiController = new ImGuiController();

        Editor = new Editor();
        Editor.Initialize();

        SceneViewController = new SceneViewController();

        SceneManager.LoadLastOpenedScene();
    }

    private static void OnWindowUpdate(FrameEventArgs e)
    {
        Time.EditorDeltaTime = (float)e.Time;

        Debug.StartGraphTimer("Editor Update", DebugGraphTimer.SourceGroup.Update, TimeSpan.FromSeconds(1f / 60f));

        Time.Update();
        MouseInput.Update();
        TweenManager.Update();
        SceneViewController.Update();
        AssetsWatcher.ProcessChangedFilesQueue();
        ShaderManager.ReloadQueuedShaders();

        SceneManager.CurrentScene.Update();
        Editor.Update();
        Debug.EndGraphTimer("Editor Update");
    }

    private static void OnWindowRender(FrameEventArgs e)
    {
        Time.EditorDeltaTime = (float)e.Time;

        Debug.StartGraphTimer("Window Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1 / 60f), -1);

        Debug.StartGraphTimer("Scene Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1f / 60f));

        RenderPassSystem.RenderAllPasses();

        Debug.EndGraphTimer("Scene Render");


        Debug.StartGraphTimer("ImGui", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromMilliseconds(2));

        ImGuiController.Update(Window, (float)e.Time);
        GL.Viewport(0, 0, Window.ClientSize.X, Window.ClientSize.Y);

        ImGuiController.WindowResized(Window.ClientSize.X, Window.ClientSize.Y);

        Editor.Draw();

        ImGuiController.Render();

        Debug.EndGraphTimer("ImGui");

        Window.SwapBuffers();

        Debug.EndGraphTimer("Window Render");

        Debug.ResetTimers();
        Debug.ClearAdditiveStats();
    }
}