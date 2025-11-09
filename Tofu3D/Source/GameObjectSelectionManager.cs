namespace TofuEngine;

public class GameObjectSelectionManager
{
    public static event Action<List<GameObject>> GameObjectsSelected;

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
            for (int i = 0; i < Tofu.SceneManager.CurrentScene.GameObjectsList.Count; i++)
            {
                if (gameObjects.Contains(Tofu.SceneManager.CurrentScene.GameObjectsList[i]) == false)
                {
                    Tofu.SceneManager.CurrentScene.GameObjectsList[i].SetSelected(false);

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
        if (Camera.ActivelyInteractedWithCamera != null)
        {
            isCameraOrTransformHandle = gameObjects.Contains(Camera.ActivelyInteractedWithCamera?.GameObject) ||
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
        for (int i = 0; i < Tofu.SceneManager.CurrentScene.GameObjectsList.Count; i++)
        {
            if (Tofu.SceneManager.CurrentScene.GameObjectsList[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    public List<GameObject> GetSelectedGameObjects()
    {
        List<GameObject> selectedGameObjects = new List<GameObject>();
        for (int i = 0; i < Tofu.SceneManager.CurrentScene.GameObjectsList.Count; i++)
        {
            if (Tofu.SceneManager.CurrentScene.GameObjectsList[i].Selected)
            {
                selectedGameObjects.Add(Tofu.SceneManager.CurrentScene.GameObjectsList[i]);
            }
        }

        return selectedGameObjects;
    }

    public GameObject? GetFirstSelectedGameObject()
    {

            if (Tofu.SceneManager.CurrentScene.SelectedGameObjectsIdList.Count>0)
            {
                int id = Tofu.SceneManager.CurrentScene.SelectedGameObjectsIdList[0];
                return Tofu.SceneManager.CurrentScene.GameObjectsDictionary[id];
            }
        

        return null;
    }
}