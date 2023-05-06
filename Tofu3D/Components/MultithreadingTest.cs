using System.Threading;

[ExecuteInEditMode]
public class MultithreadingTest : Component
{
	[Show]
	public GameObject ReferenceGameObject;
	[XmlIgnore]
	public Action ExecuteLongTaskOnMainThread;
	[XmlIgnore]
	public Action ExecuteLongTaskOnNewThread;

	public int SpawnCount = 5000;

	public override void Awake()
	{
		ExecuteLongTaskOnMainThread += ExecuteTaskOnMainThread;
		ExecuteLongTaskOnNewThread += ExecuteTaskOnNewThread;
		
		base.Awake();
	}

	private void ExecuteTaskOnMainThread()
	{
		LongTask();
	}

	private void ExecuteTaskOnNewThread()
	{
		Thread thread = new Thread(LongTask);
		thread.Start();
	}

	void LongTask()
	{
		Debug.StartTimer("Task");
		List<GameObject> gameObjects = new List<GameObject>(SpawnCount);

		for (int i = 0; i < SpawnCount; i++)
		{
			GameObject go2 = (GameObject) ReferenceGameObject.Clone();
			go2.Name = i.ToString();
			go2.Transform.LocalPosition += new Vector3(Random.Range(-10f, 10f), Random.Range(0, 10), Random.Range(0, 10));
			go2.Transform.Rotation += new Vector3(0, Random.Range(0, 360), 0);
			go2.GetComponent<Renderer>().Color = Random.RandomColor();
			gameObjects.Add(go2);
			// Debug.Log(i);
		}

		lock (SceneManager.CurrentScene.GameObjects)
		{
			SceneManager.CurrentScene.AddGameObjectsToScene(gameObjects);
		}

		Debug.EndTimer("Task");

	}
}