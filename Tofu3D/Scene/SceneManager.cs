using System.IO;

namespace Tofu3D;

public static class SceneManager
{
	public static Scene CurrentScene { get; private set; }
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
			for (int i = 0; i < CurrentScene.GameObjects.Count; i++)
			{
				CurrentScene.GameObjects[0].Destroy();
			}

			CurrentScene.GameObjects.Clear();

			CurrentScene.GameObjects = new List<GameObject>();
		}

		CurrentScene?.DisposeScene();

		CurrentScene = new Scene();
		CurrentScene.ScenePath = path;

		CurrentScene.Initialize();

		SceneFile sceneFile = AssetSerializer.LoadGameObjects(path);

		AssetSerializer.ConnectGameObjectsWithComponents(sceneFile);
		IDsManager.GameObjectNextId = sceneFile.GameObjectNextId + 1;

		AssetSerializer.ConnectParentsAndChildren(sceneFile);
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
			//sceneFile.GameObjects[i].LinkGameObjectFieldsInComponents();
			sceneFile.GameObjects[i].Awake();
		}

		Debug.EndAndLogTimer("Awake");


		for (int i = 0; i < sceneFile.GameObjects.Count; i++)
		{
			sceneFile.GameObjects[i].Start();
		}


		CurrentScene.CreateDefaultObjects();


		//
		// int lastSelectedGameObjectId = PersistentData.GetInt("lastSelectedGameObjectId", 0);
		// if (Global.EditorAttached)
		// {
		// 	EditorPanelHierarchy.I.SelectGameObject(lastSelectedGameObjectId);
		// }

		Scene.SceneLoaded.Invoke();
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
		AssetSerializer.SaveGameObjects(CurrentScene.GetSceneFile(), path);
	}
}