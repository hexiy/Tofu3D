namespace Tofu3D;

public static class Playmode
{
    public static void PlayMode_Start()
    {
        Tofu.SceneManager.SaveScene();
        Global.GameRunning = true;
        Tofu.SceneManager.LoadScene(Tofu.SceneManager.CurrentScene.ScenePath);

        // EditorPanelHierarchy.I?.SelectGameObject(-1);
    }

    public static void PlayMode_Stop()
    {
        Global.GameRunning = false;
        Tofu.SceneManager.LoadScene(Tofu.SceneManager.CurrentScene.ScenePath);
    }
}