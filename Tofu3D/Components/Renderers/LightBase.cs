
public class LightBase : Component, IRenderPassDepth
{
	public float Intensity = 1;
	public Color Color = Tofu3D.Color.White;

	public float NearPlaneDistance = 0.0001f;
	public float FarPlaneDistance = 1000;
	public float OrthographicSize = 15;
	public Vector2 Size = new Vector2(1000, 1000);

	[XmlIgnore] public static RenderTexture DepthRenderTexture { get; private set; }
	[XmlIgnore] public static RenderTexture DisplayDepthRenderTexture { get; private set; }
	
	Vector3 _cameraBeforeTransformationWorldPosition;
	Vector3 _cameraBeforeTransformationRotation;
	Vector2 _cameraBeforeTransformationSize;
	float _cameraBeforeTransformationNearPlaneDistance;
	float _cameraBeforeTransformationFarPlaneDistance;
	bool _cameraBeforeTransformationIsOrthographic;

	public static Matrix4x4 LightSpaceMatrix = Matrix4x4.Identity;

	public override void Awake()
	{
		DepthRenderTexture = new RenderTexture(size: Size, colorAttachment: false, depthAttachment: true);
		DisplayDepthRenderTexture = new RenderTexture(size: Size, colorAttachment: true, depthAttachment: false);

		RenderPassManager.RegisterRenderPassEvent(this);
		base.Awake();
	}

	public void RenderPassDepth()
	{

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

		ConfigureForShadowMapping();
		GL.ClearColor(Color.Black.ToOtherColor());
		GL.ClearDepth(100);
		GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
		DepthRenderTexture.Bind(); // start rendering to renderTexture
		GL.Viewport(0, 0, (int) Size.X, (int) Size.Y);

		
		
		//GL.Enable(EnableCap.Blend);
		
		Scene.I.RenderPassOpaques();
		
		DepthRenderTexture.Unbind(); // end rendering
		//GL.Disable(EnableCap.Blend);

// AAAH i need to update the latest view model for models, Scene.I.Update or just update the matrices


		GL.ClearColor(Color.Black.ToOtherColor());
		GL.ClearDepth(100);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		DisplayDepthRenderTexture.Bind();
		GL.Viewport(0, 0, (int) Size.X, (int) Size.Y);
		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		//  GL.ClearColor(1,0,1,1);
		// GL.ClearDepth(1);
		DisplayDepthRenderTexture.RenderDepthAttachmentFullScreen(DepthRenderTexture.DepthAttachment);
		// DisplayDepthRenderTexture.RenderDepthAttachmentFullScreen(Window.I.SceneRenderTexture.DepthAttachment);

		DisplayDepthRenderTexture.Unbind();


		ConfigureForSceneRender();

	}

	public override void Dispose()
	{
		ConfigureForSceneRender();

		base.Dispose();
	}

	private void ConfigureForShadowMapping()
	{
		_cameraBeforeTransformationWorldPosition = Camera.I.Transform.WorldPosition;
		_cameraBeforeTransformationRotation = Camera.I.Transform.Rotation;
		_cameraBeforeTransformationIsOrthographic = Camera.I.IsOrthographic;
		_cameraBeforeTransformationSize = Camera.I.Size;
		_cameraBeforeTransformationNearPlaneDistance = Camera.I.NearPlaneDistance;
		_cameraBeforeTransformationFarPlaneDistance = Camera.I.FarPlaneDistance;

		Camera.I.IsOrthographic = true;
		Camera.I.OrthographicSize = OrthographicSize;
		Camera.I.Transform.WorldPosition = Transform.WorldPosition;
		Camera.I.Transform.Rotation = Transform.Rotation;
		Camera.I.Size = Size;
		Camera.I.NearPlaneDistance = NearPlaneDistance;
		Camera.I.FarPlaneDistance = FarPlaneDistance;
		Camera.I.UpdateMatrices();



		

		// Matrix4x4 lightProjection = Matrix4x4.CreateOrthographicOffCenter(-1,0,-1,1, NearPlaneDistance, FarPlaneDistance);
		// Matrix4x4 lightView = Matrix4x4.CreateLookAt(Transform.WorldPosition,
		//                                              Transform.WorldPosition+ Transform.TransformDirectionToWorldSpace(new Vector3(0, 0, 1)),
		//                                              Transform.TransformDirectionToWorldSpace(new Vector3(0, 1, 0)));
		
		
		//
		// float left = -10;
		// float right = 10;
		// float bottom = -10;
		// float top = 10;
		//
		// Matrix4x4 lightProjection = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, NearPlaneDistance, FarPlaneDistance);
		//
		//
		// Vector3 forwardWorld = Transform.TransformDirectionToWorldSpace(new Vector3(0, 0, 1));
		// Vector3 upLocal = Transform.TransformDirectionToWorldSpace(new Vector3(0, 1, 0));
		//
		// Matrix4x4 lightView =  Matrix4x4.CreateLookAt(cameraPosition: Transform.WorldPosition, cameraTarget: forwardWorld, cameraUpVector: upLocal)
		// 	; // * Matrix4x4.CreateTranslation(Transform.WorldPosition * Units.OneWorldUnit * new Vector3(-1, -1, 1));

		
		LightSpaceMatrix = Camera.I.GetLightViewMatrix() * Camera.I.GetLightProjectionMatrix(); //lightProjection * lightView;
	}

	private void ConfigureForSceneRender()
	{
		Camera.I.IsOrthographic = _cameraBeforeTransformationIsOrthographic;
		Camera.I.Size = _cameraBeforeTransformationSize;
		Camera.I.NearPlaneDistance = _cameraBeforeTransformationNearPlaneDistance;
		Camera.I.FarPlaneDistance = _cameraBeforeTransformationFarPlaneDistance;
		Camera.I.Transform.WorldPosition = _cameraBeforeTransformationWorldPosition;
		Camera.I.Transform.Rotation = _cameraBeforeTransformationRotation;
		Camera.I.UpdateMatrices();
	}
}