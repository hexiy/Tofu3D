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
			for (int i = 0; i < Tofu.I.Scene.GameObjects.Count; i++)
			{
				if (goIds.Contains(Tofu.I.Scene.GameObjects[i].Id) == false)
				{
					Tofu.I.Scene.GameObjects[i].Selected = false;
				}
			}

			for (int i = 0; i < goIds.Count; i++)
			{
				GameObject go = Tofu.I.Scene.GetGameObject(goIds[i]);
				if (go != null)
				{
					go.Selected = true;
				}
			}
		}

		bool isCameraOrTransformHandle = false;
		if (Camera.I != null)
		{
			isCameraOrTransformHandle = goIds.Contains(Camera.I.GameObjectId) || goIds.Contains(TransformHandle.I.GameObjectId);
		}

		if (isCameraOrTransformHandle == false && goIds.Count != 0)
		{
			TransformHandle.I.SelectObjects(goIds);
			PersistentData.Set("lastSelectedGameObjectId", goIds[0]);
		}
		else
		{
			TransformHandle.I.SelectObjects(null);
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
		for (int i = 0; i < Tofu.I.Scene.GameObjects.Count; i++)
		{
			if (Tofu.I.Scene.GameObjects[i].Id == id)
			{
				return i;
			}
		}

		return -1;
	}

	public static List<GameObject> GetSelectedGameObjects()
	{
		List<GameObject> selectedGameObjects = new();
		for (int i = 0; i < Tofu.I.Scene.GameObjects.Count; i++)
		{
			if (Tofu.I.Scene.GameObjects[i].Selected)
			{
				selectedGameObjects.Add(Tofu.I.Scene.GameObjects[i]);
			}
		}

		return selectedGameObjects;
	}

	public static GameObject GetSelectedGameObject()
	{
		for (int i = 0; i < Tofu.I.Scene.GameObjects.Count; i++)
		{
			if (Tofu.I.Scene.GameObjects[i].Selected)
			{
				return Tofu.I.Scene.GameObjects[i];
			}
		}

		return null;
	}
}