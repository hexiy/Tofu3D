[ExecuteInEditMode]
public class Canvas : Component, IComponentUpdateable
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
		return;
		//Debug.Log(Transform.LocalPosition - Camera.I.Transform.LocalPosition);
		Transform.LocalPosition = Camera.MainCamera.Transform.LocalPosition;
		Transform.LocalScale = Camera.MainCamera.Transform.LocalScale;

		base.Update();
	}
}