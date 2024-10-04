using System.Threading;

[ExecuteInEditMode]
public class MultithreadingTest : Component
{
    [XmlIgnore] public Action ExecuteLongTaskOnMainThread;

    [XmlIgnore] public Action ExecuteLongTaskOnNewThread;

    [Show] public GameObject ReferenceGameObject;

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
        Thread thread = new(LongTask);
        thread.Start();
    }

    private void LongTask()
    {
        Debug.StartTimer("Task");
        List<GameObject> gameObjects = new(SpawnCount);

        for (var i = 0; i < SpawnCount; i++)
        {
            var go2 = (GameObject)ReferenceGameObject.Clone();
            go2.Name = i.ToString();
            go2.Transform.LocalPosition +=
                new Vector3(Random.Range(-10f, 10f), Random.Range(0, 10), Random.Range(0, 10));
            go2.Transform.Rotation += new Vector3(0, Random.Range(0, 360), 0);
            go2.GetComponent<Renderer>().Color = Random.RandomColor();
            gameObjects.Add(go2);
            // Debug.Log(i);
        }

        lock (Tofu.SceneManager.CurrentScene.GameObjects)
        {
            Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(gameObjects);
        }

        Debug.EndTimer("Task");
    }
}