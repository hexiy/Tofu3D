[ExecuteInEditMode]
public class ShowcaseVideoSettings : Component
{
	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		Camera.I.OrthographicSize = 1.6f;
		base.Start();
	}

	public override void Update()
	{
		base.Update();
	}
}