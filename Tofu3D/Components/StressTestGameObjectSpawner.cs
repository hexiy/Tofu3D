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
			SceneSerializer.I.SaveClipboardGameObject(Go);

			for (int i = 0; i < SpawnCount; i++)
			{
				SceneSerializer.I.LoadClipboardGameObject();
			}
		};
		Despawn += () =>
		{
			for (int j = 0; j < Scene.I.GameObjects.Count; j++)
			{
				if (j > 10)
				{
					Scene.I.GameObjects[j].Destroy();
				}
			}
		};
	}
}