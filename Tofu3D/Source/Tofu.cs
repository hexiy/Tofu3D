using System.ComponentModel;
using System.IO;
using Microsoft.Build.Locator;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TofuEngine.Physics;
using TofuEngine.Rendering;
using TofuEngine.Rendering.Instancing;
using TofuEngine.Scripting;
using TofuEngine.Tweening;

namespace TofuEngine;

// Main Application Context
internal static class Tofu
{
    // private static int _updatesThisSecond;
    // private static int _s;
    // private static int _rendersThisSecond;
    // private static int _renderS;
    // internal static Tofu I { get; private set; }

    // EDITOR
    internal static Window Window;
    internal static Editor Editor;
    internal static ImGuiController ImGuiController;
    internal static EditorSettingsAll EditorSettingsAll;
    internal static EditorWindowsManager EditorWindowsManager;
    internal static StatusWindow StatusWindow;

    // RENDERING
    internal static RenderSettings RenderSettings;
    internal static RenderPassSystem RenderPassSystem;
    internal static ShaderManager ShaderManager;
    internal static BasicMeshesCollection BasicMeshesCollection;
    internal static InstancedRenderingSystem InstancedRenderingSystem;
    internal static LightRenderingManager LightRenderingManager;

    // ASSETS
    internal static AssetImportManager AssetImportManager;

    // internal static AssetFileCache AssetFileCache;
    internal static TextureAtlasManager TextureAtlasManager;
    internal static AssetLoadManager AssetLoadManager;
    internal static SceneSerializer SceneSerializer;
    internal static AssetsWatcher AssetsWatcher;

    // SCENE
    internal static SceneManager SceneManager;
    internal static SceneViewController SceneViewController;

    // MISC
    internal static TweenManager TweenManager;
    internal static PhysicsController PhysicsController;
    internal static CoroutineManager CoroutineManager;

    // INPUT
    internal static MouseInput MouseInput;

    internal static SceneSelectionHighlighter? SceneSelectionHighlighter;
    internal static GameObjectSelectionManager GameObjectSelectionManager;

    internal static ScriptsReloader ScriptsReloader;
    internal static UserCodeEditorOpener UserCodeEditorOpener;

    internal static void Launch()
    {
        MSBuildLocator.RegisterDefaults(); // this needs to be here at start

        SystemConfig.Configure();
        Global.LoadSavedData();
        Folders.CreateDefaultFolders();

        ScriptsManager.CopyDllsToProjectFolder();
        ScriptsManager.CompileScriptsAssembly();

        UserCodeEditorOpener = new UserCodeEditorOpener();

        EditorSettingsAll = new EditorSettingsAll();
        EditorWindowsManager = new EditorWindowsManager();

        AssetImportManager = new AssetImportManager();
        // AssetFileCache = new AssetFileCache();
        TextureAtlasManager = new TextureAtlasManager();
        AssetLoadManager = new AssetLoadManager();
        SceneManager = new SceneManager();
        SceneSerializer = new SceneSerializer();
        RenderSettings = new RenderSettings();
        AssetsWatcher = new AssetsWatcher();
        ShaderManager = new ShaderManager();
        TweenManager = new TweenManager();
        MouseInput = new MouseInput();

        PhysicsController = new PhysicsController();


        RenderSettings.LoadSavedData();
        EditorSettingsAll.LoadSavedData();
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

        PhysicsController.Init();

        // first import textures so we can setup atlases
        AssetImportManager.ImportAllTextures();
        TextureAtlasManager.SetupTextureAtlases();

        // then import rest of the assets such as models/meshes that will now load correct textures from mtl files
        AssetImportManager.ImportAllAssets();


        InstancedRenderingSystem = new InstancedRenderingSystem();
        LightRenderingManager = new LightRenderingManager();

        RenderPassSystem = new RenderPassSystem();
        RenderPassSystem.Initialize();

        CoroutineManager = new CoroutineManager();

        ImGuiController = new ImGuiController();

        Editor = new Editor();
        Editor.Initialize();

        TofuImGui.Init();

        SceneViewController = new SceneViewController();

        SceneManager.LoadLastOpenedScene();

        MousePickingSystem.Initialize();

        SceneSelectionHighlighter = new SceneSelectionHighlighter();
        SceneSelectionHighlighter.Init();

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

        CoroutineManager.Update();

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
        SceneManager.CurrentScene.UploadRenderData(InstancingRenderMode.All);
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