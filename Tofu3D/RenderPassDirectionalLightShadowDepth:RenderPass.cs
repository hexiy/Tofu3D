namespace Tofu3D;

public class RenderPassDirectionalLightShadowDepth : RenderPass
{
	public static RenderPassDirectionalLightShadowDepth I { get; private set; }

	DirectionalLight _directionalLight;
	public RenderTexture DisplayDepthRenderTexture { get; private set; }
	public RenderTexture DisplayViewRenderTexture { get; private set; }

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
		PassRenderTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: true, depthAttachment: true);
		DisplayDepthRenderTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: true, depthAttachment: false);
		DisplayViewRenderTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: true, depthAttachment: false);
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
		RenderToDisplayDepthRenderTexture();
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

		DisplayViewRenderTexture.Bind();
		GL.Viewport(0, 0, (int) DisplayViewRenderTexture.Size.X, (int) DisplayViewRenderTexture.Size.Y);
		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		// GL.ClearColor(1,0,1,1);

		DisplayViewRenderTexture.RenderColorAttachment(PassRenderTexture.ColorAttachment);

		DisplayViewRenderTexture.Unbind();
	}

	private void RenderToDisplayDepthRenderTexture()
	{
		if (_directionalLight == null)
		{
			return;
		}

		GL.ClearColor(Color.Yellow.ToOtherColor());
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		DisplayDepthRenderTexture.Bind();
		GL.Viewport(0, 0, (int) DisplayDepthRenderTexture.Size.X, (int) DisplayDepthRenderTexture.Size.Y);
		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		// GL.ClearColor(1,0,1,1);

		DisplayDepthRenderTexture.RenderDepthAttachment(PassRenderTexture.DepthAttachment);

		DisplayDepthRenderTexture.Unbind();
	}
}