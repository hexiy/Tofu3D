[ExecuteInEditMode]
public class TorusRotator : Component
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
		Transform.Rotation = Transform.Rotation.Add(x: -Time.EditorDeltaTime*10);
		base.Update();
	}
}