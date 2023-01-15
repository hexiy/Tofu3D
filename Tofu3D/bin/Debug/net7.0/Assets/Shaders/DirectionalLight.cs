[ExecuteInEditMode]
public class DirectionalLight : LightBase
{
	public static DirectionalLight I { get; private set; }
	[Show]
	public float Speed = 100;

	[ExecuteInEditMode]
	public override void Update()
	{
		Transform.Rotation = Transform.Rotation.Add(y: Time.EditorDeltaTime * Speed);
		base.Update();
	}

	public override void Awake()
	{
		I = this;
		base.Awake();
	}
}