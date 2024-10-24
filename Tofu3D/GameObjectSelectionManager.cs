namespace Tofu3D;

public static class GameObjectSelectionManager
{
    public static Action<List<int>> GameObjectsSelected;

    public static void SelectGameObjects(List<int> goIds)
    {
        if (goIds == null)
        {
            goIds = new List<int>();
        }

        if (goIds != null && goIds?.Count > 0)
        {
            for (var i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
            {
                if (goIds.Contains(Tofu.SceneManager.CurrentScene.GameObjects[i].Id) == false)
                {
                    Tofu.SceneManager.CurrentScene.GameObjects[i].Selected = false;
                }
            }

            for (var i = 0; i < goIds.Count; i++)
            {
                var go = Tofu.SceneManager.CurrentScene.GetGameObject(goIds[i]);
                if (go != null)
                {
                    go.Selected = true;
                }
            }
        }

        var isCameraOrTransformHandle = false;
        if (Camera.MainCamera != null)
        {
            isCameraOrTransformHandle = goIds.Contains(Camera.MainCamera.GameObjectId) ||
                                        goIds.Contains(TransformHandle.I.GameObjectId);
        }

        if (isCameraOrTransformHandle == false && goIds.Count != 0)
        {
            TransformHandle.I.SelectObjects(goIds);
            PersistentData.Set("lastSelectedGameObjectId", goIds[0]);
        }

        // TransformHandle.I.SelectObjects(null);
        GameObjectsSelected?.Invoke(goIds);
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