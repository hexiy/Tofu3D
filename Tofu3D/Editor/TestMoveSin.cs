[ExecuteInEditMode]
public class TestMoveSin : Component
{
	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		Transform.LocalPosition = new Vector3((float) Math.Sin(Time.EditorElapsedTime * 5) * 10, 0, 0);
		base.Update();
	}
}