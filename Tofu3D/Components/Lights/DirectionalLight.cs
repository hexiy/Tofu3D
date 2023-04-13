using Tofu3D.Rendering;

[ExecuteInEditMode]
public class DirectionalLight : LightBase
{
	public static DirectionalLight I { get; private set; }
	[Show]
	public float Speed = 100;
	public bool Rotate = false;
	public int RefreshRate = 60;

	public float NearPlaneDistance = 0.0001f;
	public float FarPlaneDistance = 1000;
	public float OrthographicSize = 15;
	public Vector2 Size = new Vector2(1000, 1000);

	// [XmlIgnore] public static RenderTexture DepthRenderTexture { get; private set; }
	// [XmlIgnore] public static RenderTexture DisplayDepthRenderTexture { get; private set; }

	Vector3 _cameraBeforeTransformationWorldPosition;
	Vector3 _cameraBeforeTransformationRotation;
	Vector2 _cameraBeforeTransformationSize;
	float _cameraBeforeTransformationNearPlaneDistance;
	float _cameraBeforeTransformationFarPlaneDistance;
	bool _cameraBeforeTransformationIsOrthographic;
	float _cameraBeforeTransformationOrthographicSize;

	float _initialRotationY;

	public static Matrix4x4 LightSpaceMatrix = Matrix4x4.Identity;

	[ExecuteInEditMode]
	public override void Awake()
	{
		I = this;

		_initialRotationY = Transform.Rotation.Y;
		// DepthRenderTexture = new RenderTexture(size: Size, colorAttachment: false, depthAttachment: true);
		// DisplayDepthRenderTexture = new RenderTexture(size: Size, colorAttachment: true, depthAttachment: false);

		RenderPassSystem.RegisterRender(RenderPassType.DirectionalLightShadowDepth, RenderDirectionalLightShadowDepth);
		RenderPassDirectionalLightShadowDepth.I.SetDirectionalLight(this);

		base.Awake();
	}

	public override void Update()
	{
		if (Rotate)
		{
			Transform.Rotation = Transform.Rotation.Set(y: _initialRotationY + (float) Math.Cos(Time.EditorElapsedTime * Speed) * 5);
		}

		base.Update();
	}

	public void RenderDirectionalLightShadowDepth()
	{
		RefreshRate = Math.Clamp(RefreshRate, 1, 60);

		if (Time.EditorElapsedTicks % (60 / RefreshRate) != 0)
		{
			return;
		}
		//GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

		ConfigureForShadowMapping();

		SceneManager.CurrentScene.RenderWorld();

		ConfigureForSceneRender();
	}

	public override void OnDestroyed()
	{
		ConfigureForSceneRender();
		RenderPassSystem.RemoveRender(RenderPassType.DirectionalLightShadowDepth, RenderDirectionalLightShadowDepth);

		base.OnDestroyed();
	}

	private void ConfigureForShadowMapping()
	{
		_cameraBeforeTransformationWorldPosition = Camera.I.Transform.WorldPosition;
		_cameraBeforeTransformationRotation = Camera.I.Transform.Rotation;
		_cameraBeforeTransformationIsOrthographic = Camera.I.IsOrthographic;
		_cameraBeforeTransformationOrthographicSize = Camera.I.OrthographicSize;
		_cameraBeforeTransformationSize = Camera.I.Size;
		_cameraBeforeTransformationNearPlaneDistance = Camera.I.NearPlaneDistance;
		_cameraBeforeTransformationFarPlaneDistance = Camera.I.FarPlaneDistance;

		Camera.I.IsOrthographic = true;
		Camera.I.OrthographicSize = OrthographicSize;
		Camera.I.Transform.WorldPosition = Transform.WorldPosition * new Vector3(1, -1, 1); // TODO: well this is weird
		Camera.I.Transform.Rotation = Transform.Rotation;
		Camera.I.Size = Size;
		Camera.I.NearPlaneDistance = NearPlaneDistance;
		Camera.I.FarPlaneDistance = FarPlaneDistance;
		Camera.I.UpdateMatrices();

		LightSpaceMatrix = Camera.I.GetLightViewMatrix() * Camera.I.GetLightProjectionMatrix();
	}

	private void ConfigureForSceneRender()
	{
		Camera.I.IsOrthographic = _cameraBeforeTransformationIsOrthographic;
		Camera.I.OrthographicSize = _cameraBeforeTransformationOrthographicSize;
		Camera.I.Size = _cameraBeforeTransformationSize;
		Camera.I.NearPlaneDistance = _cameraBeforeTransformationNearPlaneDistance;
		Camera.I.FarPlaneDistance = _cameraBeforeTransformationFarPlaneDistance;
		Camera.I.Transform.WorldPosition = _cameraBeforeTransformationWorldPosition;
		Camera.I.Transform.Rotation = _cameraBeforeTransformationRotation;
		Camera.I.UpdateMatrices();
	}
}