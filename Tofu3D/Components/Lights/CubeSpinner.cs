[ExecuteInEditMode]
public class CubeSpinner : Component
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
		Transform.Rotation = Transform.Rotation.Add(y: Time.EditorDeltaTime * 20);
		base.Update();
	}
}