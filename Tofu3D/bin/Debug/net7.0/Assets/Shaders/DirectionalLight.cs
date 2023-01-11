[ExecuteInEditMode]
public class DirectionalLight : LightBase
{
	[Show]
	public float Speed = 100;
	[ExecuteInEditMode]
	public override void Update()
	{
		Transform.Rotation = Transform.Rotation.Add(y: Time.EditorDeltaTime * Speed);
		base.Update();
	}
}