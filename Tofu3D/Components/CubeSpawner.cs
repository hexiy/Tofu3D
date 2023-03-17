public class CubeSpawner : Component
{
	[Show]
	public GameObject Prefab;

	public override void Start()
	{
		SpawnCubes();
		base.Start();
	}

	void SpawnCubes()
	{
		if (Prefab == null)
		{
			return;
		}

		GameObject go = SceneSerializer.I.LoadPrefab(Prefab.PrefabPath);
		go.Awake();
SceneSerializer.I.SaveClipboardGameObject(go);
		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				GameObject go1 = SceneSerializer.I.LoadClipboardGameObject();
				go1.Transform.WorldPosition = new Vector3(x*15 - 70,-5,y*15-70);
			}
		}
	}
}