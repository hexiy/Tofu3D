using OpenTK.Windowing.Common;
using Tofu3D.Tweening;

namespace Tofu3D;

// Main Application Context
public class Tofu
{
	SceneSerializer _sceneSerializer;
	TweenManager _tweenManager;
	SceneViewNavigation _sceneViewNavigation;
	Scene _scene;

	public void Launch()
	{
		SystemConfig.Configure();
		Global.LoadSavedData();

		Window window = new Window();
		window.Load += () => { OnWindowLoad(window); };
		window.UpdateFrame += OnWindowUpdate;
		window.Run();
	}

	void OnWindowUpdate(FrameEventArgs obj)
	{
		Time.Update();
		MouseInput.Update();
		TweenManager.I.Update();
		_sceneViewNavigation.Update();
		ShaderCache.ReloadQueuedShaders();

		_scene.Update();
	}

	void OnWindowLoad(Window window)
	{
		Editor editor = new Editor();
		editor.Init();

		_sceneSerializer = new SceneSerializer();
		_tweenManager = new TweenManager();
		_sceneViewNavigation = new SceneViewNavigation();

		_scene = new Scene();
		_scene.Initialize();

		window.UpdateFrame += _ => _scene.Update();
	}
}