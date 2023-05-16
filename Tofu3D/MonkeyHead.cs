[ExecuteInEditMode]
public class MonkeyHead : Component, IComponentUpdateable
{
	Renderer _renderer;

	public override void Awake()
	{
		_renderer = GetComponent<Renderer>();
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		Color color = Extensions.ColorFromHsv(Time.EditorElapsedTime*200, 0.6f, 1);
		_renderer.Color = color;
		base.Update();
	}
}