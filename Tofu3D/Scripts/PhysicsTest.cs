using Tofu3D.Physics;

public class PhysicsTest : Component, IComponentUpdateable
{
	BoxShape _boxShape;
	ModelRenderer _modelRenderer;

	public override void Awake()
	{
		_boxShape = GetComponent<BoxShape>();
		_modelRenderer = GetComponent<ModelRenderer>();
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		if (_boxShape == null || _modelRenderer == null)
		{
			return;
		}

		// Ray ray = new Ray(Camera.I.ScreenToWorld(Tofu.MouseInput.ScreenPosition)*1000, Camera.I.TransformToWorld(Vector3.Forward).Normalized());
		Ray ray = new(Camera.MainCamera.TransformToWorld(Tofu.MouseInput.ScreenPosition), Camera.MainCamera.TransformToWorld(Tofu.MouseInput.ScreenPosition) + Camera.MainCamera.TransformToWorld(Vector3.Forward).Normalized());

		RaycastResult result = Physics.Raycast(ray);

		//Debug.Log("Center of screen in world:"+Camera.I.CenterOfScreenToWorld());
		Debug.Log("Mouse world pos:" + ray.Origin);
		// if (result.hitBodies.Count > 0)
		// {
		// 	modelRenderer.color = Color.Red;
		// }
		// else
		// {
		// 	modelRenderer.color = Color.White;
		// }

		base.Update();
	}
}