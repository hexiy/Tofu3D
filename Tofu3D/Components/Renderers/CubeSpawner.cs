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

		GameObject go = Serializer.I.LoadPrefab(_prefab.PrefabPath);
		go.Awake();
Serializer.I.SaveClipboardGameObject(go);
		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				GameObject go1 = Serializer.I.LoadClipboardGameObject();
				go1.Transform.WorldPosition = new Vector3(x*15 - 70,-5,y*15-70);
			}
		}
	}
}