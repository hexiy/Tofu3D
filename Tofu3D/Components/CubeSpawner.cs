public class CubeSpawner : Component
{
    [Show] public GameObject Prefab;

    public override void Start()
    {
        SpawnCubes();
        base.Start();
    }

    private void SpawnCubes()
    {
        if (Prefab == null)
        {
            return;
        }

        var go = Tofu.SceneSerializer.LoadPrefab(Prefab.PrefabPath);
        go.Awake();
        Tofu.SceneSerializer.SaveClipboardGameObject(go);
        for (var x = 0; x < 10; x++)
        for (var y = 0; y < 10; y++)
        {
            var go1 = Tofu.SceneSerializer.LoadClipboardGameObject();
            go1.Transform.WorldPosition = new Vector3(x * 15 - 70, -5, y * 15 - 70);
        }
    }
}