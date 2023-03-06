/*namespace Engine;

public class MousePickingSensor : Component
{
	Renderer _renderer;

	public override void Awake()
	{
		_renderer = GetComponent<Renderer>();
		base.Awake();
	}

	public override void Update()
	{
		if (_renderer == MousePickingSystem.HoveredRenderer)
		{
			Transform.Rotation = new Vector3(Rendom.Range(-100, 100), Rendom.Range(-100, 100), Rendom.Range(-100, 100));
		}
		base.Update();
	}
}*/