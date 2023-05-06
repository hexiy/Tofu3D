﻿using System.Collections.Concurrent;
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
	public Action Despawn;
	public int TerrainSize = 10;
	public int ThreadsToUse = 2;
	float _cubeModelSize = 2;

	bool _savedToClipboard = false;

	public override void Awake()
	{
		Spawn += StartTerrainGenerationOnNewThread;
		Despawn += DestroyTerrain;
		SceneSerializer.SaveClipboardGameObject(CubePrefab);
		base.Awake();
	}

	void DestroyTerrain()
	{
		while (Transform.Children.Count > 0)
		{
			Transform.Children[0].GameObject.Destroy();
		}
	}

	public override void Update()
	{
		if (_threadsWorkingCount == 0)
		{
			_threadsWorkingCount = -1;
			
		}

		base.Update();
	}

	int _threadsWorkingCount = -1;

	private void StartTerrainGenerationOnNewThread()
	{
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

			// go.GetComponent<Renderer>().Color = Random.RandomColor();
			_concurrentBag.Enqueue(go);
		}


		Debug.EndAndLogTimer($"Thread #{threadIndex} finished");

		_threadsWorkingCount--;
		if (_threadsWorkingCount == 0)
		{
			int x = 0;
			int z = 0;

			SceneManager.CurrentScene.AddGameObjectsToScene(_concurrentBag);

			foreach (GameObject go in _concurrentBag)
			{
				go.Transform.SetParent(Transform);

				float positionY = Mathf.Sin((float) x / TerrainSize * 10f) * Mathf.Cos((float) z / TerrainSize * 10f) * 5;
				positionY = positionY.TranslateToGrid(2);
				go.Transform.LocalPosition = new Vector3(x * _cubeModelSize, positionY, z * _cubeModelSize);


				x++;
				if (x > TerrainSize)
				{
					x = 0;
					z++;
				}
			}

			Debug.EndAndLogTimer($"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");
		}
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