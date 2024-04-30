public class Benchmark : Component, IComponentUpdateable
{
    public override void Awake()
    {
        base.Awake();
    }

    private void SpawnSpriteRenderer()
    {
        Vector3 position = new(Random.Range(-100, 100), Random.Range(0, 200), 0);
        GameObject go = GameObject.Create(position, name: $"benchmarkSprite{IDsManager.GameObjectNextId}");
        BoxShape boxShape = go.AddComponent<BoxShape>();
        boxShape.Size = new Vector3(5, 5, 1);
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.LoadTexture("2D/house.png");

        go.Awake();
    }

    public override void Start()
    {
        for (int i = 0; i < 10_000; i++) SpawnSpriteRenderer();

        base.Start();
    }

    public void Update()
    {
    }
}