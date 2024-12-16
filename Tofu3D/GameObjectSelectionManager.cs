namespace Tofu3D;

public static class GameObjectSelectionManager
{
    public static Action<List<GameObject>> GameObjectsSelected;

    private static readonly List<GameObject> _singleGameObjectList = new List<GameObject>(1) { null };

    public static void SelectGameObject(GameObject go)
    {
        _singleGameObjectList[0] = go;
        SelectGameObjects(_singleGameObjectList);
    }

    public static void SelectGameObjects(List<GameObject> gameObjects)
    {
        if (gameObjects == null)
        {
            gameObjects = new List<GameObject>();
        }

        if (gameObjects != null && gameObjects?.Count > 0)
        {
            for (var i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
            {
                if (gameObjects.Contains(Tofu.SceneManager.CurrentScene.GameObjects[i]) == false)
                {
                    Tofu.SceneManager.CurrentScene.GameObjects[i].Selected = false;
                }
            }

            for (var i = 0; i < gameObjects.Count; i++)
            {
                // var go = Tofu.SceneManager.CurrentScene.GetGameObjectByID(gameObjects[i]);
                // if (go != null)
                // {
                // go.Selected = true;
                // }
                gameObjects[i].Selected = true;
            }
        }

        var isCameraOrTransformHandle = false;
        if (Camera.MainCamera != null)
        {
            isCameraOrTransformHandle = gameObjects.Contains(Camera.MainCamera.GameObject) ||
                                        gameObjects.Contains(TransformHandle.I.GameObject);
        }

        if (isCameraOrTransformHandle == false && gameObjects.Count != 0)
        {
            TransformHandle.I.SelectObjects(gameObjects);
            PersistentData.Set("lastSelectedGameObjectId", gameObjects[0].Id);
        }

        // TransformHandle.I.SelectObjects(null);
        GameObjectsSelected?.Invoke(gameObjects);
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

    public static int GetGameObjectIndexInHierarchy(int id)
    {
        for (var i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
        {
            if (Tofu.SceneManager.CurrentScene.GameObjects[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    public static List<GameObject> GetSelectedGameObjects()
    {
        List<GameObject> selectedGameObjects = new();
        for (var i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
        {
            if (Tofu.SceneManager.CurrentScene.GameObjects[i].Selected)
            {
                selectedGameObjects.Add(Tofu.SceneManager.CurrentScene.GameObjects[i]);
            }
        }

        return selectedGameObjects;
    }

    public static GameObject GetSelectedGameObject()
    {
        for (var i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
        {
            if (Tofu.SceneManager.CurrentScene.GameObjects[i].Selected)
            {
                return Tofu.SceneManager.CurrentScene.GameObjects[i];
            }
        }

        return null;
    }
}