using System.IO;

namespace Tofu3D;

public static class SceneManager
{
	public static Scene CurrentScene { get; private set; }
	// public PersistentObject<string> LastOpenedScene = ("lastOpenedScene", "Assets/Scenes/scene1.scene");
	public static string LastOpenedScene
	{
		get { return PersistentData.GetString("lastOpenedScene", "Assets/Scenes/scene1.scene"); }
		set { PersistentData.Set("lastOpenedScene", value); }
	}

	public static void LoadLastOpenedScene()
	{
		LoadScene(SceneManager.LastOpenedScene);
	}

	public static bool LoadScene(string path = null)
	{
		Debug.ClearLogs();

		Debug.StartTimer("LoadScene");


		LastOpenedScene = path;
		// Tofu.I.Window.Title = Tofu.I.Window.WindowTitleText + " | " + Path.GetFileNameWithoutExtension(path);


		//Add method to clean scene
		if (CurrentScene != null)
		{
			CurrentScene?.DisposeScene();

			
		}


		CurrentScene = new Scene();
		CurrentScene.ScenePath = path;

		CurrentScene.Initialize();

		SceneFile sceneFile = SceneSerializer.LoadSceneFile(path);

		SceneSerializer.ConnectGameObjectsWithComponents(sceneFile);
		IDsManager.GameObjectNextId = sceneFile.GameObjectNextId + 1;

		SceneSerializer.ConnectParentsAndChildren(sceneFile);
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
			sceneFile.GameObjects[i].Awake();
		}

		Debug.EndAndLogTimer("Awake");


		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			sceneFile.GameObjects[i].Start();
		}

		CurrentScene.CreateDefaultObjects();

		Scene.AnySceneLoaded.Invoke();
		Debug.EndAndLogTimer("LoadScene");

		return true;
	}

	public static void SaveScene(string path = null)
	{
		path = path ?? LastOpenedScene;
		if (path.Length < 1)
		{
			path = Path.Combine("Assets", "scene1.scene");
		}

		LastOpenedScene = path;
		SceneSerializer.SaveGameObjects(CurrentScene.GetSceneFile(), path);
	}
}