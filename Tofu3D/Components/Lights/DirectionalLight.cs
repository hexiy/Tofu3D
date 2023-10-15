using Tofu3D.Rendering;

[ExecuteInEditMode]
public class DirectionalLight : LightBase
{
	[Show]
	public float Speed = 100;
	public bool Rotate = false;

	public float RotateOffset = 0;

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

	public static Matrix4x4 LightSpaceViewProjectionMatrix = Matrix4x4.Identity;
	public static DirectionalLight I { get; private set; } = null;
	[ExecuteInEditMode]
	public override void Awake()
	{
		// DepthRenderTexture = new RenderTexture(size: Size, colorAttachment: false, depthAttachment: true);
		// DisplayDepthRenderTexture = new RenderTexture(size: Size, colorAttachment: true, depthAttachment: false);

		Tofu.RenderPassSystem.RegisterRender(RenderPassType.DirectionalLightShadowDepth, RenderDirectionalLightShadowDepth);
		RenderPassDirectionalLightShadowDepth.I?.SetDirectionalLight(this);
		base.Awake();
	}

	public override void OnDisabled()
	{
		I = null;
		base.OnDisabled();
	}

	public override void OnEnabled()
	{
		I = this;
		base.OnEnabled();
	}

	public override void Update()
	{
		if (Rotate)
		{
			Transform.Rotation = Transform.Rotation.Set(x: (float) Mathf.SinAbs(Time.EditorElapsedTime*0.5f)+0.2f* 30,y: RotateOffset + (float) Math.Sin(Time.EditorElapsedTime * Speed) * 50);
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

		ConfigureForShadowMapping();

		Tofu.SceneManager.CurrentScene.RenderWorld();

		ConfigureForSceneRender();
	}

	public override void OnDestroyed()
	{
		ConfigureForSceneRender();
		Tofu.RenderPassSystem.RemoveRender(RenderPassType.DirectionalLightShadowDepth, RenderDirectionalLightShadowDepth);

		base.OnDestroyed();
	}

	private void ConfigureForShadowMapping()
	{
		_cameraBeforeTransformationWorldPosition = Camera.MainCamera.Transform.WorldPosition;
		_cameraBeforeTransformationRotation = Camera.MainCamera.Transform.Rotation;
		_cameraBeforeTransformationIsOrthographic = Camera.MainCamera.IsOrthographic;
		_cameraBeforeTransformationOrthographicSize = Camera.MainCamera.OrthographicSize;
		_cameraBeforeTransformationSize = Camera.MainCamera.Size;
		_cameraBeforeTransformationNearPlaneDistance = Camera.MainCamera.NearPlaneDistance;
		_cameraBeforeTransformationFarPlaneDistance = Camera.MainCamera.FarPlaneDistance;

		Camera.MainCamera.IsOrthographic = true;
		Camera.MainCamera.OrthographicSize = OrthographicSize;
		Camera.MainCamera.Transform.WorldPosition = Transform.WorldPosition; // * new Vector3(1, 1, 1); // TODO: well this is weird
		Camera.MainCamera.Transform.Rotation = Transform.Rotation;
		Camera.MainCamera.Size = Size;
		Camera.MainCamera.NearPlaneDistance = NearPlaneDistance;
		Camera.MainCamera.FarPlaneDistance = FarPlaneDistance;
		Camera.MainCamera.UpdateMatrices();

		LightSpaceViewProjectionMatrix = Camera.MainCamera.GetLightViewMatrix() * Camera.MainCamera.GetLightProjectionMatrix(OrthographicSize);
	}

	private void ConfigureForSceneRender()
	{
		Camera.MainCamera.IsOrthographic = _cameraBeforeTransformationIsOrthographic;
		Camera.MainCamera.OrthographicSize = _cameraBeforeTransformationOrthographicSize;
		Camera.MainCamera.Size = _cameraBeforeTransformationSize;
		Camera.MainCamera.NearPlaneDistance = _cameraBeforeTransformationNearPlaneDistance;
		Camera.MainCamera.FarPlaneDistance = _cameraBeforeTransformationFarPlaneDistance;
		Camera.MainCamera.Transform.WorldPosition = _cameraBeforeTransformationWorldPosition;
		Camera.MainCamera.Transform.Rotation = _cameraBeforeTransformationRotation;
		Camera.MainCamera.UpdateMatrices();
	}
}