namespace Scripts;

public class InspectorObjectReferencesTest : Component
{
	[XmlIgnore]
	public Action AddGameObjectToList;
	[Show]
	public List<GameObject> Gos = new();
	List<Vector2> _mouseCachedPositions = new();

	Vector2 _snakePos;

	public override void Awake()
	{
		//gos = new List<GameObject>();
		AddGameObjectToList = () =>
		{
			GameObject go = GameObject.Create(name: "testGO" + Tofu.SceneManager.CurrentScene.GameObjects.Count);
			go.Transform.Pivot = new Vector3(0.5f, 0.5f, 0.5f);
			BoxShape boxShape = go.AddComponent<BoxShape>();
			boxShape.Size = new Vector2(100, 100);
			BoxRenderer boxRenderer = go.AddComponent<BoxRenderer>();
			boxRenderer.Layer = 100 - Gos.Count;
			boxRenderer.Color = Extensions.ColorFromHsv(Gos.Count * 33f, 0.7f, 0.8f);
			go.Awake();
			Gos.Add(go);
		};
		base.Awake();
	}

	public override void Update()
	{
		base.Update();
	}
}