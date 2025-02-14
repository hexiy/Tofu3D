namespace TofuEngine;

public class GameObjectSelectionManager
{
    public static Action<List<GameObject>> GameObjectsSelected;

    private int LastSelectedGameObjectId
    {
        get { return PersistentData.GetInt("LastSelectedGameObjectId", -1); }
        set { PersistentData.Set("LastSelectedGameObjectId", value); }
    }

    private readonly List<GameObject> _singleGameObjectList = new List<GameObject>(1) { null };
    private readonly List<GameObject> _emptyGameObjectList = new List<GameObject>(0) { };

    public GameObjectSelectionManager()
    {
        Scene.SceneLoaded += SelectLastSelectedGameObject;
    }

    ~GameObjectSelectionManager()
    {
        Scene.SceneLoaded -= SelectLastSelectedGameObject;
    }

    public void SelectGameObject(GameObject go)
    {
        if (go == null)
        {
            Deselect();
            return;
        }

        _singleGameObjectList[0] = go;
        SelectGameObjects(_singleGameObjectList);
    }

    public void SelectLastSelectedGameObject()
    {
        if (LastSelectedGameObjectId == -1)
        {
            return;
        }

        GameObject go = Tofu.SceneManager.CurrentScene.GetGameObjectByID(LastSelectedGameObjectId);
        if (go == null)
        {
            return;
        }

        SelectGameObject(go);
    }

    public void SelectGameObjects(List<GameObject> gameObjects)
    {
        if (gameObjects == null)
        {
            gameObjects = new List<GameObject>();
        }

        if (gameObjects != null && gameObjects?.Count > 0)
        {
            for (int i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
            {
                if (gameObjects.Contains(Tofu.SceneManager.CurrentScene.GameObjects[i]) == false)
                {
                    Tofu.SceneManager.CurrentScene.GameObjects[i].SetSelected(false);

                }
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                // var go = Tofu.SceneManager.CurrentScene.GetGameObjectByID(gameObjects[i]);
                // if (go != null)
                // {
                // go.Selected = true;
                // }
                gameObjects[i].SetSelected(true);
            }
        }

        bool isCameraOrTransformHandle = false;
        if (Camera.MainCamera != null)
        {
            isCameraOrTransformHandle = gameObjects.Contains(Camera.MainCamera?.GameObject) ||
                                        gameObjects.Contains(TransformHandle.I?.GameObject);
        }

        if (isCameraOrTransformHandle == false && gameObjects.Count != 0)
        {
            LastSelectedGameObjectId = gameObjects[0].Id;
        }

        // TransformHandle.I.SelectObjects(null);
        GameObjectsSelected?.Invoke(gameObjects);
    }

    public void Deselect()
    {
        LastSelectedGameObjectId = -1;
        GameObjectsSelected?.Invoke(_emptyGameObjectList);
    }
    // static void OnGameObjectSelected(List<int> ids)
    // {
    // 	if (Global.EditorAttached == false)
    // 	{
    // 		ids = null;
    // 	}
    //
    // 	if (ids == null)
    // 	{
    // 		SelectGameObjects(null);
    // 	}
    // 	else
    // 	{
    // 		// if (GetGameObjectIndexInHierarchy(ids) == -1)
    // 		// {
    // 		// 	return;
    // 		// }
    //
    // 		SelectGameObjects(ids);
    // 	}
    // }

    public int GetGameObjectIndexInHierarchy(int id)
    {
        for (int i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
        {
            if (Tofu.SceneManager.CurrentScene.GameObjects[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    public List<GameObject> GetSelectedGameObjects()
    {
        List<GameObject> selectedGameObjects = new List<GameObject>();
        for (int i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
        {
            if (Tofu.SceneManager.CurrentScene.GameObjects[i].Selected)
            {
                selectedGameObjects.Add(Tofu.SceneManager.CurrentScene.GameObjects[i]);
            }
        }

        return selectedGameObjects;
    }

    public GameObject GetSelectedGameObject()
    {
        for (int i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
        {
            if (Tofu.SceneManager.CurrentScene.GameObjects[i].Selected)
            {
                return Tofu.SceneManager.CurrentScene.GameObjects[i];
            }
        }

        return null;
    }
}