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
	float _cubeModelSize = 2;

	bool _savedToClipboard = false;
	PerlinNoise _perlinNoise;

	public override void Awake()
	{
		_perlinNoise = new PerlinNoise((int) (Time.EditorDeltaTime * 100000));
		Spawn += GenerateTerrain;
		Despawn += DestroyTerrain;
	}

	void DestroyTerrain()
	{
		while (Transform.Children.Count > 0)
		{
			Transform.Children[0].GameObject.Destroy();
		}
	}

	public override void Start()
	{
		// Spawn();
	}

	private void GenerateTerrain()
	{
		if (_savedToClipboard == false)
		{
			SceneSerializer.SaveClipboardGameObject(CubePrefab);
		}


		int x = 0;
		int z = 0;
		for (int i = 0; i < TerrainSize * TerrainSize; i++)
		{
			GameObject go = SceneSerializer.LoadClipboardGameObject();
			go.Transform.SetParent(Transform);

			float positionY = Mathf.Sin((float) x / TerrainSize * 5f) * Mathf.Cos((float) z / TerrainSize * 10f)*5;
			positionY = positionY.TranslateToGrid(2);
			go.Transform.LocalPosition = new Vector3(x * _cubeModelSize, positionY, z * _cubeModelSize);

			// go.GetComponent<Renderer>().Color = Random.RandomColor();

			x++;
			if (x > TerrainSize)
			{
				x = 0;
				z++;
			}
		}
	}
}