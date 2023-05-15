using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tofu3D;

[ExecuteInEditMode]
public class TerrainGenerator : Component
{
	public GameObject CubePrefab;
	[XmlIgnore]
	public Action Spawn;
	[XmlIgnore]
	public Action SpawnSingleThreaded;
	[XmlIgnore]
	public Action Despawn;
	public int TerrainSize = 10;
	public int ThreadsToUse = 2;
	float _cubeModelSize = 2;

	bool _savedToClipboard = false;

	public override void Awake()
	{
		Spawn += StartTerrainGenerationOnNewThread;
		SpawnSingleThreaded += StartTerrainGenerationOnNewThread;
		Despawn += DestroyTerrain;
		base.Awake();
	}

	public override void Start()
	{
		// Spawn.Invoke();
		base.Start();
	}

	void DestroyTerrain()
	{
		for (int i = 0; i < Transform.Children.Count; i++)
		{
			Transform.Children[0].GameObject.Destroy();
		}

		Transform.Children = new List<Transform>();
	}

	public override void Update()
	{
		if (_threadsWorkingCount == 0)
		{
			_threadsWorkingCount = -1;
			AddBlocksToScene();
		}

		if (KeyboardInput.WasKeyJustPressed(Keys.Space))
		{
			Spawn?.Invoke();
		}

		base.Update();
	}

	int _threadsWorkingCount = -1;

	private void StartTerrainGenerationOnNewThread()
	{
		if (CubePrefab == null)
		{
			return;
		}

		SceneSerializer.SaveClipboardGameObject(CubePrefab);

		DestroyTerrain();
		_concurrentBag.Clear();

		Debug.StartTimer($"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");

		int numberOfThreads = ThreadsToUse;
		_threadsWorkingCount = numberOfThreads;
		List<Thread> threads = new List<Thread>();
		for (int threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
		{
			int capturedThreadIndex = threadIndex;
			Thread thread = new Thread(() => GenerateTerrain(TerrainSize, CubePrefab, capturedThreadIndex, numberOfThreads));
			threads.Add(thread);
		}

		threads.ForEach(t => t.Start());
	}

	private void StartTerrainGenerationOnMainThread()
	{
		if (CubePrefab == null)
		{
			return;
		}

		SceneSerializer.SaveClipboardGameObject(CubePrefab);

		DestroyTerrain();
		_concurrentBag.Clear();

		GenerateTerrain(TerrainSize, CubePrefab, 0, 1);
	}

	ConcurrentQueue<GameObject> _concurrentBag = new ConcurrentQueue<GameObject>();

	private void GenerateTerrain(int terrainSize, GameObject referenceGameObject, int threadIndex, int numberOfThreads)
	{
		Debug.StartTimer($"Thread #{threadIndex} finished");

		int totalBlocks = terrainSize * terrainSize;
		int blocksPerThread = totalBlocks / numberOfThreads;
		int startIndex = blocksPerThread * threadIndex;
		int endIndex = blocksPerThread + (threadIndex * blocksPerThread);


		for (int i = startIndex; i < endIndex; i++)
		{
			// Debug.Log(i);
			GameObject go = (GameObject) referenceGameObject.Clone();
			go.Name = $"Thread:{threadIndex} go {i}";
			go.DynamicallyCreated = true;

			_concurrentBag.Enqueue(go);
		}


		Debug.EndAndLogTimer($"Thread #{threadIndex} finished");

		_threadsWorkingCount--;
		if (_threadsWorkingCount == 0)
		{
			// AddBlocksToScene();
		}
	}

	private void AddBlocksToScene()
	{
		int x = 0;
		int z = 0;

		SceneManager.CurrentScene.AddGameObjectsToScene(_concurrentBag);
		float peak = Random.Range(10, 20);
		Vector2 flatSurfaceAreaPosition = new Vector2(30, 30);
		foreach (GameObject go in _concurrentBag)
		{
			go.Transform.SetParent(Transform);

			float positionY = Mathf.Sin((float) x / 10f) * Mathf.Cos((float) z / 10) * 15;
			positionY = positionY.TranslateToGrid(2);

			go.Transform.LocalPosition = new Vector3(x * _cubeModelSize, positionY, z * _cubeModelSize);
			float distToFlatSurface = Vector2.Distance(flatSurfaceAreaPosition, new Vector2(go.Transform.WorldPosition.X, go.Transform.WorldPosition.Z));
			float maxDist = 100;
			if (distToFlatSurface < maxDist)
			{
				positionY = Mathf.Lerp(positionY, 0, (maxDist - distToFlatSurface)/maxDist);
			}


			x++;
			if (x > TerrainSize)
			{
				x = 0;
				z++;
			}

			peak += Random.Range(-0.01f, 0.01f);
			peak = Mathf.Clamp(peak, 10, 25);
		}

		Debug.EndAndLogTimer($"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");
	}

	void LongTask()
	{
		List<GameObject> gameObjects = new List<GameObject>(20000);
		for (int i = 0; i < 20000; i++)
		{
			GameObject go = GameObject.Create(name: i.ToString(), addToScene: false);
			gameObjects.Add(go);
			Debug.Log(i);
		}

		lock (SceneManager.CurrentScene.GameObjects)
		{
			SceneManager.CurrentScene.AddGameObjectsToScene(gameObjects);
		}
	}
}