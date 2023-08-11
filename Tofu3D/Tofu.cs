using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tofu3D.Rendering;
using Tofu3D.Tweening;

namespace Tofu3D;

// Main Application Context
public class Tofu
{
	public static Tofu I { get; private set; }

	// EDITOR
	public Window Window;
	public Editor Editor;
	public ImGuiController ImGuiController;

	// RENDERING
	public RenderSettings RenderSettings;
	public RenderPassSystem RenderPassSystem;
	public ShaderManager ShaderManager;
	public InstancedRenderingSystem InstancedRenderingSystem;

	// ASSETS
	public AssetManager AssetManager;
	public SceneSerializer SceneSerializer;
	public AssetsWatcher AssetsWatcher;

	// SCENE
	public SceneManager SceneManager;
	public SceneViewController SceneViewController;

	// MISC
	public TweenManager TweenManager;
	
	// INPUT
	public MouseInput MouseInput;

	public Tofu()
	{
		I = this;
	}

	public void Launch()
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

	void OnWindowLoad()
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

	void OnWindowUpdate(FrameEventArgs e)
	{
		Time.EditorDeltaTime = (float) (e.Time);

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

	void OnWindowRender(FrameEventArgs e)
	{
		Time.EditorDeltaTime = (float) (e.Time);

		Debug.StartGraphTimer("Window Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1 / 60f), -1);

		Debug.StartGraphTimer("Scene Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1f / 60f));

		RenderPassSystem.RenderAllPasses();

		Debug.EndGraphTimer("Scene Render");


		Debug.StartGraphTimer("ImGui", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromMilliseconds(2));

		ImGuiController.Update(Window, (float) e.Time);
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