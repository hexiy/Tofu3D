using Tofu3D.Physics;

public class PhysicsTest : Component, IComponentUpdateable
{
    private BoxShape _boxShape;
    private ModelRenderer _modelRenderer;

    public void Update()
    {
        if (_boxShape == null || _modelRenderer == null)
        {
            return;
        }

        // Ray ray = new Ray(Camera.I.ScreenToWorld(Tofu.MouseInput.ScreenPosition)*1000, Camera.I.TransformToWorld(Vector3.Forward).Normalized());
        Ray ray = new(Camera.MainCamera.TransformToWorld(Tofu.MouseInput.PositionInView),
            Camera.MainCamera.TransformToWorld(Tofu.MouseInput.PositionInView) +
            Camera.MainCamera.TransformToWorld(Vector3.Forward).Normalized());

        var result = Physics.Raycast(ray);

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
    }

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
}