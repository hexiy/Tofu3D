using System.Collections.Concurrent;
using System.Threading;

namespace Tofu3D;

[ExecuteInEditMode]
public class TerrainGenerator : Component, IComponentUpdateable
{
    private readonly ConcurrentQueue<GameObject> _concurrentBag = new();
    private readonly float _cubeModelSize = 2;

    [Show] private float _huh = 2;

    private bool _savedToClipboard = false;

    private int _threadsWorkingCount = -1;
    public GameObject CubePrefab;

    [XmlIgnore] public Action Despawn;

    [XmlIgnore] public Action Spawn;

    [XmlIgnore] public Action SpawnSingleThreaded;

    public int TerrainSize = 10;
    public int ThreadsToUse = 2;

    public void Update()
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
    }

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
        for (var i = 0; i < Transform.Children.Count; i++)
        {
            Transform.Children[0].GameObject.Destroy();
        }

        Transform.Children = new List<Transform>();
    }

    private void StartTerrainGenerationOnNewThread()
    {
        if (CubePrefab == null)
        {
            return;
        }

        Tofu.SceneSerializer.SaveClipboardGameObject(CubePrefab);

        DestroyTerrain();
        _concurrentBag.Clear();

        Debug.StartTimer(
            $"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");

        var numberOfThreads = ThreadsToUse;
        _threadsWorkingCount = numberOfThreads;
        List<Thread> threads = new();
        for (var threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
        {
            var capturedThreadIndex = threadIndex;
            Thread thread = new(() => GenerateTerrain(TerrainSize, CubePrefab, capturedThreadIndex, numberOfThreads));
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

        Tofu.SceneSerializer.SaveClipboardGameObject(CubePrefab);

        DestroyTerrain();
        _concurrentBag.Clear();

        GenerateTerrain(TerrainSize, CubePrefab, 0, 1);
    }

    private void GenerateTerrain(int terrainSize, GameObject referenceGameObject, int threadIndex, int numberOfThreads)
    {
        Debug.StartTimer($"Thread #{threadIndex} finished");

        var totalBlocks = terrainSize * terrainSize;
        var blocksPerThread = totalBlocks / numberOfThreads;
        var startIndex = blocksPerThread * threadIndex;
        var endIndex = blocksPerThread + threadIndex * blocksPerThread;


        for (var i = startIndex; i < endIndex; i++)
        {
            // Debug.Log(i);
            var go = (GameObject)referenceGameObject.Clone(false);
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
        var x = 0;
        var z = 0;

        Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(_concurrentBag);
        foreach (var go in _concurrentBag)
        {
            go.Transform.SetParent(Transform);

            var positionY = Mathf.Sin(x / 10f) * Mathf.Cos((float)z / 10) * 15;
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
        for (var i = 0; i < 20000; i++)
        {
            var go = GameObject.Create(name: i.ToString(), addToScene: false);
            gameObjects.Add(go);
            Debug.Log(i);
        }

        lock (Tofu.SceneManager.CurrentScene.GameObjects)
        {
            Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(gameObjects);
        }
    }
}