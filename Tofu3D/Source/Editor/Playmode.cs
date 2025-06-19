namespace TofuEngine;

public static class Playmode
{
    public static bool GameRunning = false;

    public static void PlayMode_Start()
    {
        // Tofu.SceneManager.SaveScene();
        GameRunning = true;
        Tofu.SceneManager.LoadScene(Tofu.SceneManager.CurrentScene.ScenePath);

        // EditorPanelHierarchy.I?.SelectGameObject(-1);
    }

    public static void PlayMode_Stop()
    {
        GameRunning = false;
        Tofu.SceneManager.LoadScene(Tofu.SceneManager.CurrentScene.ScenePath);
    }
}