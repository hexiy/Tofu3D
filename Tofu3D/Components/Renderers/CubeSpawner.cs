public class CubeSpawner : Component
{
	[Show]
	public GameObject _prefab;

	public override void Start()
	{
		SpawnCubes();
		base.Start();
	}

	void SpawnCubes()
	{
		if (_prefab == null)
		{
			return;
		}

		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				GameObject go = Serializer.I.LoadPrefab(_prefab.PrefabPath);
				go.Awake();
				go.Transform.WorldPosition = new Vector3(Rendom.Range(-3, 3), Rendom.Range(-3, 3), Rendom.Range(-3, 3));
			}
		}
	}
}