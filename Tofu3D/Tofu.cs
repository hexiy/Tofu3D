using OpenTK.Windowing.Common;
using Tofu3D.Tweening;

namespace Tofu3D;

// Main Application Context
public class Tofu
{
	// AssetSerializer _assetSerializer;
	public TweenManager TweenManager;
	public SceneViewNavigation SceneViewNavigation;
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

		Window = new Window();
		Window.Load += () => { OnWindowLoad(Window); };
		Window.UpdateFrame += OnWindowUpdate;
		Window.Run();
	}

	void OnWindowUpdate(FrameEventArgs obj)
	{
		Time.Update();
		MouseInput.Update();
		TweenManager.I.Update();
		SceneViewNavigation.Update();
		ShaderCache.ReloadQueuedShaders();

		SceneManager.CurrentScene.Update();
		Editor.Update();
	}

	void OnWindowLoad(Window window)
	{
		Editor = new Editor();
		Editor.Init();

		// _assetSerializer = new AssetSerializer();
		TweenManager = new TweenManager();
		SceneViewNavigation = new SceneViewNavigation();


		SceneManager.LoadLastOpenedScene();
		// Scene = new Scene();
		// Scene.Initialize();
	}
}