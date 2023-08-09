namespace Tofu3D;

public static class Playmode
{
	public static void PlayMode_Start()
	{
		Tofu.I.SceneManager.SaveScene();
		Global.GameRunning = true;
		Tofu.I.SceneManager.LoadScene(Tofu.I.SceneManager.CurrentScene.ScenePath);

		EditorPanelHierarchy.I?.SelectGameObject(-1);
	}

	public static void PlayMode_Stop()
	{
		Global.GameRunning = false;
		Tofu.I.SceneManager.LoadScene(Tofu.I.SceneManager.CurrentScene.ScenePath);
	}

	static void SaveCurrentSceneBeforePlay()
	{
	}

	static void LoadSceneSavedBeforePlay()
	{
	}
}