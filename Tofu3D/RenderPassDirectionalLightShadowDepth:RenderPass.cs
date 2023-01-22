namespace Tofu3D;

public class RenderPassDirectionalLightShadowDepth : RenderPass
{
	public static RenderPassDirectionalLightShadowDepth I { get; private set; }

	DirectionalLight _directionalLight;

	// shadows depth map
	public RenderTexture DepthMapRenderTexture { get; private set; }

	// renders camera's color view, for debug
	public RenderTexture LightDebugViewColorRenderTexture { get; private set; }

	public RenderPassDirectionalLightShadowDepth() : base(RenderPassType.DirectionalLightShadowDepth)
	{
		I = this;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public void SetDirectionalLight(DirectionalLight directionalLight)
	{
		_directionalLight = directionalLight;

		SetupRenderTexture();
	}

	protected override void SetupRenderTexture()
	{
		// PassRenderTexture contains the depth, we render that depth with DeptRenderTexture.glsl shader to DepthMapRenderTexture and use that as a shadowmap
		PassRenderTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: true, depthAttachment: true);
		DepthMapRenderTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: true, depthAttachment: false);
		LightDebugViewColorRenderTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: true, depthAttachment: false);
		// cannot write from depth to color on one framebuffer right?

		base.SetupRenderTexture();
	}

	protected override void PreRender()
	{
		base.PreRender();
	}

	protected override void PostRender()
	{
		base.PostRender();
		RenderToDepthMapRenderTexture();
		//RenderToDisplayViewRenderTexture();
	}

	void RenderToDisplayViewRenderTexture()
	{
		if (_directionalLight == null)
		{
			return;
		}

		GL.ClearColor(Color.Yellow.ToOtherColor());
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		LightDebugViewColorRenderTexture.Bind();
		GL.Viewport(0, 0, (int) LightDebugViewColorRenderTexture.Size.X, (int) LightDebugViewColorRenderTexture.Size.Y);
		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		// GL.ClearColor(1,0,1,1);

		LightDebugViewColorRenderTexture.RenderColorAttachment(PassRenderTexture.ColorAttachment);

		LightDebugViewColorRenderTexture.Unbind();
	}

	private void RenderToDepthMapRenderTexture()
	{
		if (_directionalLight == null)
		{
			return;
		}

		GL.ClearColor(Color.Yellow.ToOtherColor());
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		DepthMapRenderTexture.Bind();
		GL.Viewport(0, 0, (int) DepthMapRenderTexture.Size.X, (int) DepthMapRenderTexture.Size.Y);
		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		// GL.ClearColor(1,0,1,1);

		DepthMapRenderTexture.RenderDepthAttachment(PassRenderTexture.DepthAttachment);

		DepthMapRenderTexture.Unbind();
	}
}