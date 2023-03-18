namespace Tofu3D;

public static class Playmode
{
	public static void PlayMode_Start()
	{
		SceneManager.SaveScene();
		Global.GameRunning = true;
		SceneManager.LoadScene(SceneManager.CurrentScene.ScenePath);

		EditorPanelHierarchy.I?.SelectGameObject(-1);
	}

	public static void PlayMode_Stop()
	{
		Global.GameRunning = false;
		SceneManager.LoadScene(SceneManager.CurrentScene.ScenePath);
	}

	static void SaveCurrentSceneBeforePlay()
	{
	}

	static void LoadSceneSavedBeforePlay()
	{
	}
}