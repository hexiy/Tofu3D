using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tofu3D.Rendering;
using Tofu3D.Tweening;

namespace Tofu3D;

// Main Application Context
public class Tofu
{
	// AssetSerializer _assetSerializer;
	public TweenManager TweenManager;
	public SceneViewController SceneViewController;
	public Editor Editor;
	public Window Window;

	public static Tofu I { get; private set; }

	public Tofu()
	{
		I = this;
	}

	public void Launch()
	{
		SystemConfig.Configure();
		Global.LoadSavedData();

		SceneSerializer.Initialize();
		RenderSettings.LoadSavedData();
		AssetsWatcher.StartWatching();

		Window = new Window();
		Window.Load += () => { OnWindowLoad(Window); };
		Window.UpdateFrame += OnWindowUpdate;
		Window.Run();
	}

	void OnWindowLoad(Window window)
	{
		try
		{
			RenderPassSystem.Initialize();

		}
		catch (Exception ex)
		{
			throw ex;
		}

		Editor = new Editor();
		Editor.Init();

		// _assetSerializer = new AssetSerializer();
		TweenManager = new TweenManager();
		SceneViewController = new SceneViewController();


		SceneManager.LoadLastOpenedScene();
		// Scene = new Scene();
		// Scene.Initialize();
	}

	void OnWindowUpdate(FrameEventArgs obj)
	{
		Debug.StartGraphTimer("Editor Update", DebugGraphTimer.SourceGroup.Update, TimeSpan.FromSeconds(1f / 60f));

		Time.Update();
		MouseInput.Update();
		TweenManager.I.Update();
		SceneViewController.Update();
		ShaderCache.ReloadQueuedShaders();

		SceneManager.CurrentScene.Update();
		Editor.Update();
		Debug.EndGraphTimer("Editor Update");
	}
}