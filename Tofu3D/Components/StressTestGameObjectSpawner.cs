namespace Tofu3D;

[ExecuteInEditMode]
public class StressTestGameObjectSpawner : Component
{
    private readonly bool _savedToClipboard = false;

    [XmlIgnore] public Action Despawn;

    public GameObject Go;

    [XmlIgnore] public Action Spawn;

    public int SpawnCount = 1000;

    public override void Awake()
    {
        Spawn += () =>
        {
            if (_savedToClipboard == false)
            {
                Tofu.SceneSerializer.SaveClipboardGameObject(Go);
            }

            var timerName = "StressTest";
            Debug.StartTimer(timerName);


            for (var i = 0; i < SpawnCount; i++)
            {
                // GameObject go = SceneSerializer.Experimental_LoadClipboardGameObject();
                var go = Tofu.SceneSerializer.LoadClipboardGameObject();
                go.Transform.LocalPosition +=
                    new Vector3(Random.Range(-10f, 10f), Random.Range(0, 10), Random.Range(0, 10));
                go.Transform.Rotation += new Vector3(0, Random.Range(0, 360), 0);
                go.GetComponent<Renderer>().Color = Random.RandomColor();
            }

            var duration = Debug.EndTimer(timerName);

            Debug.Log($"Spawning {SpawnCount} objects took {duration} ms, {duration / SpawnCount} ms for 1 object");
        };
        Despawn += () =>
        {
            for (var j = 0; j < Tofu.SceneManager.CurrentScene.GameObjects.Count; j++)
            {
                if (j > 10)
                {
                    Tofu.SceneManager.CurrentScene.GameObjects[j].Destroy();
                }
            }
        };
    }
}