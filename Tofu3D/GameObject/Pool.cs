namespace Scripts;

public class Pool
{
	public Stack<GameObject> FreeObjects = new();
	public GameObject Model;
	public Stack<GameObject> UsedObjects = new();

	void AddNewObject()
	{
		GameObject gameObject = GameObject.Create(name: "Pooled object");
		for (int i = 0; i < Model.Components.Count; i++)
		{
			gameObject.AddComponent(Model.Components[i].GetType());
		}

		gameObject.Awake();
		FreeObjects.Push(gameObject);
	}

	public GameObject Request()
	{
		if (FreeObjects.Count == 0)
		{
			AddNewObject();
		}

		GameObject gameObject = FreeObjects.Pop();
		gameObject.SetActive(true);
		UsedObjects.Push(gameObject);
		return gameObject;
	}

	public void Return(GameObject gameObject)
	{
		gameObject.SetActive(false);
		FreeObjects.Push(gameObject);
	}
}