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
			for (int i = 0; i < SceneManager.CurrentScene.GameObjects.Count; i++)
			{
				if (goIds.Contains(SceneManager.CurrentScene.GameObjects[i].Id) == false)
				{
					SceneManager.CurrentScene.GameObjects[i].Selected = false;
				}
			}

			for (int i = 0; i < goIds.Count; i++)
			{
				GameObject go = SceneManager.CurrentScene.GetGameObject(goIds[i]);
				if (go != null)
				{
					go.Selected = true;
				}
			}
		}

		bool isCameraOrTransformHandle = false;
		if (Camera.MainCamera != null)
		{
			isCameraOrTransformHandle = goIds.Contains(Camera.MainCamera.GameObjectId) || goIds.Contains(TransformHandle.I.GameObjectId);
		}

		if (isCameraOrTransformHandle == false && goIds.Count != 0)
		{
			TransformHandle.I.SelectObjects(goIds);
			PersistentData.Set("lastSelectedGameObjectId", goIds[0]);
		}
		else
		{
			// TransformHandle.I.SelectObjects(null);
		}

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
		for (int i = 0; i < SceneManager.CurrentScene.GameObjects.Count; i++)
		{
			if (SceneManager.CurrentScene.GameObjects[i].Id == id)
			{
				return i;
			}
		}

		return -1;
	}

	public static List<GameObject> GetSelectedGameObjects()
	{
		List<GameObject> selectedGameObjects = new();
		for (int i = 0; i < SceneManager.CurrentScene.GameObjects.Count; i++)
		{
			if (SceneManager.CurrentScene.GameObjects[i].Selected)
			{
				selectedGameObjects.Add(SceneManager.CurrentScene.GameObjects[i]);
			}
		}

		return selectedGameObjects;
	}

	public static GameObject GetSelectedGameObject()
	{
		for (int i = 0; i < SceneManager.CurrentScene.GameObjects.Count; i++)
		{
			if (SceneManager.CurrentScene.GameObjects[i].Selected)
			{
				return SceneManager.CurrentScene.GameObjects[i];
			}
		}

		return null;
	}
}