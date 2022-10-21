namespace Tofu3D;

public static class Playmode
{
	public static void PlayMode_Start()
	{
		Scene.I.SaveScene();
		Global.GameRunning = true;
		Scene.I.LoadScene(Scene.I.ScenePath);

		EditorPanelHierarchy.I?.SelectGameObject(-1);
	}

	public static void PlayMode_Stop()
	{
		Global.GameRunning = false;
		Scene.I.LoadScene(Scene.I.ScenePath);
	}

	static void SaveCurrentSceneBeforePlay()
	{
	}

	static void LoadSceneSavedBeforePlay()
	{
	}
}