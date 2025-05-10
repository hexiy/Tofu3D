using System.IO;

namespace TofuEngine;

public class SceneManager
{
    public Scene CurrentScene { get; private set; }

    // public PersistentObject<string> LastOpenedScene = ("lastOpenedScene", "Assets/Scenes/scene1.scene");
    public string LastOpenedSceneName
    {
        get => PersistentData.GetString("lastOpenedScene", "defaultScene.scene");
        set => PersistentData.Set("lastOpenedScene", value);
    }

    public string LastOpenedScenePath => TofuPath.Combine(Folders.ScenesInAssets, LastOpenedSceneName);

    public void LoadLastOpenedScene()
    {
        LoadScene(LastOpenedScenePath);
    }

    public void ReloadScene()
    {
        LoadScene(CurrentScene.ScenePath);
    }

    public bool LoadScene(string path = null)
    {
        // Debug.ClearLogs();

        Debug.StartTimer("LoadScene");


        // Tofu.Window.Title = Tofu.Window.WindowTitleText + " | " + Path.GetFileNameWithoutExtension(path);


        //Add method to clean scene
        if (CurrentScene != null)
        {
            CurrentScene?.DisposeScene();
        }


        CurrentScene = new Scene();

        if (path == null && File.Exists(LastOpenedSceneName) == false)
        {
            path = TofuPath.Combine(Folders.Assets, "Scenes", "defaultScene.scene");
            CurrentScene.SetupAndSaveEmptyScene(path);
        }

        CurrentScene.ScenePath = path;

        CurrentScene.Initialize();

        SceneFile sceneFile = Tofu.SceneSerializer.LoadSceneFile(path);

        Tofu.SceneSerializer.ConnectGameObjectsWithComponents(sceneFile);
        IDsManager.GameObjectNextId = sceneFile.GameObjectNextId + 1;

        Tofu.SceneSerializer.ConnectParentsAndChildren(sceneFile);
        for (int i = 0; i < sceneFile.GameObjects.Count; i++)
        {
            for (int j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
            {
                sceneFile.GameObjects[i].Components[j].GameObjectId = sceneFile.GameObjects[i].Id;
            }

            CurrentScene.AddGameObjectToScene(sceneFile.GameObjects[i]);
        }

        Debug.StartTimer("Awake");
        for (int i = 0; i < sceneFile.GameObjects.Count; i++)
        {
            sceneFile.GameObjects[i].LinkGameObjectFieldsInComponents();
            sceneFile.GameObjects[i].Awake(callStartAfterAwake: false);
        }

        Debug.EndAndLogTimer("Awake");


        for (int i = 0; i < sceneFile.GameObjects.Count; i++)
        {
            if (sceneFile.GameObjects[i].ActiveInHierarchy)
            {
                sceneFile.GameObjects[i].Start();
            }
        }

        CurrentScene.CreateDefaultObjects();

        Scene.OnSceneLoaded();
        Debug.EndAndLogTimer("LoadScene");

        LastOpenedSceneName = Path.GetFileName(path);

        return true;
    }

    public void SaveScene(string path = null)
    {
        path = path ?? LastOpenedScenePath;
        if (path.Length < 1)
        {
            path = TofuPath.Combine(Folders.ScenesInAssets, "defaultScene.scene");
        }

        Tofu.SceneSerializer.SaveGameObjects(CurrentScene.GetSceneFile(), path);

        LastOpenedSceneName = Path.GetFileName(path);

        FramebufferScreenshotGenerator.TakeScreenshot(Tofu.RenderingSystem.SceneViewPipeline.FinalFramebuffer,
            fileName: CurrentScene.ThumbnailPath, scale: 0.2f);
    }
}