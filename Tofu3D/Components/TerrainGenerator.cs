using System.Collections.Concurrent;
using System.Threading;

namespace Tofu3D;

[ExecuteInEditMode]
public class TerrainGenerator : Component, IComponentUpdateable
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
    private float _cubeModelSize = 2;

    [Show]
    private float _huh = 2;

    private bool _savedToClipboard = false;

    public override void Awake()
    {
        Spawn += StartTerrainGenerationOnNewThread;
        SpawnSingleThreaded += StartTerrainGenerationOnNewThread;
        Despawn += DestroyTerrain;
        base.Awake();
    }

    /*public override void Start()
    {
        Spawn.Invoke();
        base.Start();
    }*/

    private void DestroyTerrain()
    {
        for (int i = 0; i < Transform.Children.Count; i++) Transform.Children[0].GameObject.Destroy();

        Transform.Children = new List<Transform>();
    }

    public void Update()
    {
        if (_threadsWorkingCount == 0)
        {
            _threadsWorkingCount = -1;
            AddBlocksToScene();
        }

        if (KeyboardInput.WasKeyJustPressed(Keys.Space)) Spawn?.Invoke();

    }

    private int _threadsWorkingCount = -1;

    private void StartTerrainGenerationOnNewThread()
    {
        if (CubePrefab == null) return;

        Tofu.SceneSerializer.SaveClipboardGameObject(CubePrefab);

        DestroyTerrain();
        _concurrentBag.Clear();

        Debug.StartTimer(
            $"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");

        int numberOfThreads = ThreadsToUse;
        _threadsWorkingCount = numberOfThreads;
        List<Thread> threads = new();
        for (int threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
        {
            int capturedThreadIndex = threadIndex;
            Thread thread = new(() => GenerateTerrain(TerrainSize, CubePrefab, capturedThreadIndex, numberOfThreads));
            threads.Add(thread);
        }

        threads.ForEach(t => t.Start());
    }

    private void StartTerrainGenerationOnMainThread()
    {
        if (CubePrefab == null) return;

        Tofu.SceneSerializer.SaveClipboardGameObject(CubePrefab);

        DestroyTerrain();
        _concurrentBag.Clear();

        GenerateTerrain(TerrainSize, CubePrefab, 0, 1);
    }

    private ConcurrentQueue<GameObject> _concurrentBag = new();

    private void GenerateTerrain(int terrainSize, GameObject referenceGameObject, int threadIndex, int numberOfThreads)
    {
        Debug.StartTimer($"Thread #{threadIndex} finished");

        int totalBlocks = terrainSize * terrainSize;
        int blocksPerThread = totalBlocks / numberOfThreads;
        int startIndex = blocksPerThread * threadIndex;
        int endIndex = blocksPerThread + threadIndex * blocksPerThread;


        for (int i = startIndex; i < endIndex; i++)
        {
            // Debug.Log(i);
            GameObject go = (GameObject)referenceGameObject.Clone(false);
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

        Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(_concurrentBag);
        foreach (GameObject go in _concurrentBag)
        {
            go.Transform.SetParent(Transform);

            float positionY = Mathf.Sin((float)x / 10f) * Mathf.Cos((float)z / 10) * 15;
            positionY = positionY.TranslateToGrid(2);

            go.Transform.LocalPosition = new Vector3(x * _cubeModelSize, positionY, z * _cubeModelSize);
            go.SetActive(true);
            x++;
            if (x > TerrainSize)
            {
                x = 0;
                z++;
            }
        }

        Debug.EndAndLogTimer(
            $"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");
    }

    private void LongTask()
    {
        List<GameObject> gameObjects = new(20000);
        for (int i = 0; i < 20000; i++)
        {
            GameObject go = GameObject.Create(name: i.ToString(), addToScene: false);
            gameObjects.Add(go);
            Debug.Log(i);
        }

        lock (Tofu.SceneManager.CurrentScene.GameObjects)
        {
            Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(gameObjects);
        }
    }
}