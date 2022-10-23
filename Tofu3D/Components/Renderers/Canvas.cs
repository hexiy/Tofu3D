[ExecuteInEditMode]
public class Canvas : Component
{
	public static Canvas I { get; private set; }

	public override void Awake()
	{
		I = this;
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		Debug.Log(Transform.LocalPosition - Camera.I.Transform.LocalPosition);
		Transform.LocalPosition = Camera.I.Transform.LocalPosition;
		Transform.Scale = Camera.I.Transform.Scale;

		base.Update();
	}
}