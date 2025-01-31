using System.Collections.Concurrent;
using System.Threading;

namespace Tofu3D;

[ExecuteInEditMode]
public class StressTestGameObjectSpawner : Component
{
    private readonly bool _savedToClipboard = false;

    [XmlIgnore]
    public Action Despawn;

    // public GameObject Go;

    [XmlIgnore]
    public Action Spawn;

    public int SpawnCount = 1000;

    public float Radius = 100;

    public override void Awake()
    {
        Spawn += StartSpawningOnNewThread;
        Despawn += Destroy;
        
        base.Awake();
    }


    private readonly ConcurrentQueue<GameObject> _concurrentBag = new ConcurrentQueue<GameObject>();

    private int _threadsWorkingCount = -1;

    public int ThreadsToUse = 2;

    public void Update()
    {
        if (_threadsWorkingCount == 0)
        {
            _threadsWorkingCount = -1;
            AddObjectsToScene();
        }

        if (KeyboardInput.WasKeyJustPressed(Keys.Space))
        {
            Spawn?.Invoke();
        }
    }

    /*public override void Start()
    {
        Spawn.Invoke();
        base.Start();
    }*/

    private void Destroy()
    {
        for (int i = 0; i < Transform.Children.Count; i++)
        {
            Transform.Children[0].GameObject.Destroy();
        }

        Transform.Children = new List<Transform>();
    }

    private void StartSpawningOnNewThread()
    {
        // if (_savedToClipboard == false)
        // {
        //     Tofu.SceneSerializer.SaveClipboardGameObject(Transform.Children[0].GameObject);
        // }


        for (int i = 0; i < SpawnCount; i++)
        {
            // var go = Tofu.SceneSerializer.LoadClipboardGameObject();
            // go.Transform.SetParent(Transform);
            // go.Transform.LocalPosition +=
            //     new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f),
            //         Random.Range(-1f, 1f)) * Radius;
            // go.Transform.Rotation += new Vector3(Random.Range(0, 360), Random.Range(0, 360), 0);
        }

        // var duration = Debug.EndTimer(timerName);

        // Debug.Log($"Spawning {SpawnCount} objects took {duration} ms, {duration / SpawnCount} ms for 1 object");
//////////

        Destroy();
        _concurrentBag.Clear();

        string timerName = "StressTest";
        Debug.StartTimer(timerName);

        int numberOfThreads = ThreadsToUse;
        _threadsWorkingCount = numberOfThreads;
        List<Thread> threads = new List<Thread>();
        GameObject go = Transform.Children[0].GameObject;
        for (int threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
        {
            int capturedThreadIndex = threadIndex;
            Thread thread = new Thread(() => SpawnObjects(SpawnCount, go, capturedThreadIndex, numberOfThreads));
            threads.Add(thread);
        }

        threads.ForEach(t => t.Start());
    }

    private void SpawnObjects(int count, GameObject referenceGameObject, int threadIndex, int numberOfThreads)
    {
        Debug.StartTimer($"Thread #{threadIndex} finished");

        int objectsPerThread = count / numberOfThreads;
        int startIndex = objectsPerThread * threadIndex;
        int endIndex = objectsPerThread + threadIndex * objectsPerThread;


        for (int i = startIndex; i < endIndex; i++)
        {
            // Debug.Log(i);
            GameObject go = (GameObject)referenceGameObject.Clone(false);
            go.Name = $"Thread:{threadIndex} go {i}";
            go.RuntimeOnly = true;

            _concurrentBag.Enqueue(go);
        }


        Debug.EndAndLogTimer($"Thread #{threadIndex} finished");

        _threadsWorkingCount--;
        if (_threadsWorkingCount == 0)
        {
            // AddObjectsToScene();
        }
    }

    private void AddObjectsToScene()
    {
        Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(_concurrentBag);
        foreach (GameObject go in _concurrentBag)
        {
            go.Transform.SetParent(Transform);
            go.Transform.LocalPosition +=
                new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)) * Radius;
            go.Transform.Rotation += new Vector3(Random.Range(0, 360), Random.Range(0, 360), 0);

            go.SetActive(true);
        }

        float duration = Debug.EndTimer("StressTest");
        Debug.Log($"Spawning {SpawnCount} objects took {duration} ms, {duration / SpawnCount} ms for 1 object");
    }

    private void LongTask()
    {
        List<GameObject> gameObjects = new List<GameObject>(20000);
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