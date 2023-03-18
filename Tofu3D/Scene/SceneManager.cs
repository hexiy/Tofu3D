using System.IO;

namespace Tofu3D;

public static class SceneManager
{
	static Scene _currentScene;
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
		if (_currentScene != null)
		{
			for (int i = 0; i < _currentScene.GameObjects.Count; i++)
			{
				_currentScene.GameObjects[0].Destroy();

			}

			_currentScene.GameObjects.Clear();

			_currentScene.GameObjects = new List<GameObject>();
		}

		_currentScene?.DisposeScene();

		_currentScene = new Scene();
		Tofu.I.Scene = _currentScene;
		_currentScene.Initialize();

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

			_currentScene.AddGameObjectToScene(sceneFile.GameObjects[i]);
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


		_currentScene.CreateDefaultObjects();

		_currentScene.ScenePath = path;
		Tofu.I.Scene = _currentScene;

		//
		// int lastSelectedGameObjectId = PersistentData.GetInt("lastSelectedGameObjectId", 0);
		// if (Global.EditorAttached)
		// {
		// 	EditorPanelHierarchy.I.SelectGameObject(lastSelectedGameObjectId);
		// }


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
		AssetSerializer.SaveGameObjects(_currentScene.GetSceneFile(), path);
	}
}