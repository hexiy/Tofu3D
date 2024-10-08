using System.IO;

namespace Tofu3D;

public class SceneManager
{
    public Scene CurrentScene { get; private set; }

    // public PersistentObject<string> LastOpenedScene = ("lastOpenedScene", "Assets/Scenes/scene1.scene");
    public string LastOpenedScene
    {
        get => PersistentData.GetString("lastOpenedScene", "Assets/Scenes/scene1.scene");
        set => PersistentData.Set("lastOpenedScene", value);
    }

    public void LoadLastOpenedScene()
    {
        LoadScene(LastOpenedScene);
    }

    public bool LoadScene(string path = null)
    {
        Debug.ClearLogs();

        Debug.StartTimer("LoadScene");


        LastOpenedScene = path;
        // Tofu.Window.Title = Tofu.Window.WindowTitleText + " | " + Path.GetFileNameWithoutExtension(path);


        //Add method to clean scene
        if (CurrentScene != null)
        {
            CurrentScene?.DisposeScene();
        }


        CurrentScene = new Scene();

        if (path == null || File.Exists(LastOpenedScene) == false)
        {
            CurrentScene.SetupAndSaveEmptyScene(Path.Combine("Assets", "Scenes", "scene0.scene"));
        }

        CurrentScene.ScenePath = path;

        CurrentScene.Initialize();

        var sceneFile = Tofu.SceneSerializer.LoadSceneFile(path);

        Tofu.SceneSerializer.ConnectGameObjectsWithComponents(sceneFile);
        IDsManager.GameObjectNextId = sceneFile.GameObjectNextId + 1;

        Tofu.SceneSerializer.ConnectParentsAndChildren(sceneFile);
        for (var i = 0; i < sceneFile.GameObjects.Count; i++)
        {
            for (var j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
            {
                sceneFile.GameObjects[i].Components[j].GameObjectId = sceneFile.GameObjects[i].Id;
            }

            CurrentScene.AddGameObjectToScene(sceneFile.GameObjects[i]);
        }

        Debug.StartTimer("Awake");
        for (var i = 0; i < sceneFile.GameObjects.Count; i++)
        {
            sceneFile.GameObjects[i].LinkGameObjectFieldsInComponents();
            sceneFile.GameObjects[i].Awake();
        }

        Debug.EndAndLogTimer("Awake");


        for (var i = 0; i < sceneFile.GameObjects.Count; i++)
        {
            sceneFile.GameObjects[i].Start();
        }

        CurrentScene.CreateDefaultObjects();

        Scene.AnySceneLoaded.Invoke();
        Debug.EndAndLogTimer("LoadScene");

        return true;
    }

    public void SaveScene(string path = null)
    {
        path = path ?? LastOpenedScene;
        if (path.Length < 1)
        {
            path = Path.Combine("Assets", "scene1.scene");
        }

        LastOpenedScene = path;
        Tofu.SceneSerializer.SaveGameObjects(CurrentScene.GetSceneFile(), path);
    }
}