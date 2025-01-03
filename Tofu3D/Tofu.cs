using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Locator;
using OpenTK.Windowing.Common;
using Tofu3D.Rendering;
using Tofu3D.Scripting;
using Tofu3D.Tweening;

namespace Tofu3D;

// Main Application Context
public static class Tofu
{
    // private static int _updatesThisSecond;
    // private static int _s;
    // private static int _rendersThisSecond;
    // private static int _renderS;
    // public static Tofu I { get; private set; }

    // EDITOR
    public static Window Window;
    public static Editor Editor;
    public static ImGuiController ImGuiController;

    // RENDERING
    public static RenderSettings RenderSettings;
    public static RenderPassSystem RenderPassSystem;
    public static ShaderManager ShaderManager;
    public static BasicMeshesCollection BasicMeshesCollection;
    public static InstancedRenderingSystem InstancedRenderingSystem;

    // ASSETS
    public static AssetImportManager AssetImportManager;
    public static AssetLoadManager AssetLoadManager;
    public static SceneSerializer SceneSerializer;
    public static AssetsWatcher AssetsWatcher;

    // SCENE
    public static SceneManager SceneManager;
    public static SceneViewController SceneViewController;

    // MISC
    public static TweenManager TweenManager;

    // INPUT
    public static MouseInput MouseInput;

    public static SceneSelectionHighlighter SceneSelectionHighlighter;
    public static GameObjectSelectionManager GameObjectSelectionManager;

    public static ScriptsReloader ScriptsReloader;

    public static void Launch()
    {
        MSBuildLocator.RegisterDefaults(); // this needs to be here at start

        SystemConfig.Configure();
        Global.LoadSavedData();
        Folders.CreateDefaultFolders();

        ScriptsManager.CopyDllsToProjectFolder();
        ScriptsManager.CompileScriptsAssembly();

        AssetImportManager = new AssetImportManager();
        AssetLoadManager = new AssetLoadManager();
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

        ScriptsReloader = new ScriptsReloader();

        Window = new Window();
        Window.Load += OnWindowLoad;
        Window.UpdateFrame += MainLoop;
        // Window.RenderFrame += OnWindowRender;
        Window.Closing += OnWindowClosing;
        Window.Run();
    }

    private static void OnWindowClosing(CancelEventArgs obj)
    {
        if (Directory.Exists(Folders.TempInLibrary))
        {
            Directory.Delete(Folders.TempInLibrary, recursive: true);
            Directory.CreateDirectory(Folders.TempInLibrary);
        }
    }

    private static void MainLoop(FrameEventArgs eventArgs)
    {
        OnWindowUpdate(eventArgs);
        OnWindowRender(eventArgs);
    }

    private static void OnWindowLoad()
    {
        BasicMeshesCollection = new BasicMeshesCollection();

        AssetImportManager.ImportAllAssets();

        InstancedRenderingSystem = new InstancedRenderingSystem();

        RenderPassSystem = new RenderPassSystem();
        RenderPassSystem.Initialize();


        ImGuiController = new ImGuiController();

        Editor = new Editor();
        Editor.Initialize();

        SceneViewController = new SceneViewController();

        SceneManager.LoadLastOpenedScene();

        MousePickingSystem.Initialize();

        // SceneSelectionHighlighter = new SceneSelectionHighlighter();
        // SceneSelectionHighlighter.Init();

        GameObjectSelectionManager = new GameObjectSelectionManager();
    }

    // static Stopwatch sw = new Stopwatch();

    private static void OnWindowUpdate(FrameEventArgs e)
    {
        // if (Window.FrameLimiterEnabled)
        // {
        // Ensure updates are limited to the specified frame rate
        // }
        // Time.EditorDeltaTime = (float)sw.Elapsed.TotalSeconds;
        Time.EditorDeltaTime = (float)e.Time;
        if (Time.EditorDeltaTime == 0)
        {
            Time.EditorDeltaTime = 1f / 60f;
        }

        // sw.Restart();
        // // Time.EditorDeltaTime = (float)e.Time;
        // if (DateTime.Now.Second == _s)
        // {
        //     _updatesThisSecond++;
        // }
        // else
        // {
        //     Debug.StatSetValue("Updates per second:", "Updates per second:" + _updatesThisSecond);
        //     _s = DateTime.Now.Second;
        //     _updatesThisSecond = 0;
        // }

        Debug.StartGraphTimer("Editor Update", DebugGraphTimer.SourceGroup.Update, TimeSpan.FromSeconds(1f / 120f));
        ImGuiController.Update(Window, Time.EditorDeltaTime);

        Time.Update();
        MouseInput.Update();
        TweenManager.Update();
        SceneViewController.Update();
        SceneSelectionHighlighter?.Update();
        MousePickingSystem.Update();
        AssetsWatcher.ProcessChangedFilesQueue();
        ShaderManager.ReloadQueuedShaders();

        SceneManager.CurrentScene.Update();
        Editor.Update();
        Debug.EndGraphTimer("Editor Update");

        ScriptsReloader.ReloadScriptsIfNeeded();


        // if (KeyboardInput.WasKeyJustPressed(Keys.Enter))
        // {
        // FramebufferScreenshotGenerator.TakeScreenshot(RenderPassSystem.FinalFramebuffer);
        // }
    }

    private static void OnWindowRender(FrameEventArgs e)
    {
        // if (DateTime.Now.Second == _renderS)
        // {
        //     _rendersThisSecond++;
        // }
        // else
        // {
        //     Debug.StatSetValue("Renders per second:", "Renders per second:" + _rendersThisSecond);
        //     _renderS = DateTime.Now.Second;
        //     _rendersThisSecond = 0;
        // }

        // Time.EditorDeltaTime = (float)e.Time;

        Debug.StartGraphTimer("Window Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1 / 120f), -1);

        Debug.StartGraphTimer("Scene Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1f / 120f));
        Camera.MainCamera.UpdateMatrices();

        RenderPassSystem.RenderAllPasses();

        Debug.EndGraphTimer("Scene Render");


        Debug.StartGraphTimer("ImGui", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromMilliseconds(2));

        GL.Viewport(0, 0, Window.ClientSize.X, Window.ClientSize.Y);

        ImGuiController.WindowResized(Window.ClientSize.X, Window.ClientSize.Y);

        Editor.Draw();

        ImGuiController.Render();

        Debug.EndGraphTimer("ImGui");

        Window.SwapBuffers();

        Debug.EndGraphTimer("Window Render");

        Debug.ResetTimers();
        Debug.ClearAdditiveStats();


        Window.ManageFrameLimiter();
    }
}