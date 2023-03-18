namespace Tofu3D;
[ExecuteInEditMode]
public class StressTestGameObjectSpawner : Component
{
	[XmlIgnore]
	public Action Despawn;

	public GameObject Go;
	[XmlIgnore]
	public Action Spawn;
	public int SpawnCount = 1000;

	public override void Awake()
	{
		Spawn += () =>
		{
			AssetSerializer.SaveClipboardGameObject(Go);

			for (int i = 0; i < SpawnCount; i++)
			{
				AssetSerializer.LoadClipboardGameObject();
			}
		};
		Despawn += () =>
		{
			for (int j = 0; j < SceneManager.CurrentScene.GameObjects.Count; j++)
			{
				if (j > 10)
				{
					SceneManager.CurrentScene.GameObjects[j].Destroy();
				}
			}
		};
	}
}