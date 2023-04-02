using Tofu3D.Rendering;

namespace Tofu3D;

public class RenderPassDirectionalLightShadowDepth : RenderPass
{
	public static RenderPassDirectionalLightShadowDepth I { get; private set; }

	DirectionalLight _directionalLight;

	// shadows depth map
	public RenderTexture DepthMapRenderTexture { get; private set; }

	public RenderPassDirectionalLightShadowDepth() : base(RenderPassType.DirectionalLightShadowDepth)
	{
		I = this;
	}

	protected override bool CanRender()
	{
		return _directionalLight?.IsActive == true;
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
		PassRenderTexture.ClearColor = new Color(0, 0, 0, 255);
		DepthMapRenderTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: true, depthAttachment: false);

		base.SetupRenderTexture();
	}

	protected override void PreRender()
	{
		// it would be nice to render the skybox to the light view preview textures
		// RenderPassSkybox.I.Render();
		// RenderPassSkybox.I.RenderToFramebuffer(PassRenderTexture, FramebufferAttachment.Color);
		base.PreRender();
	}

	protected override void PostRender()
	{
		base.PostRender();
		RenderToDebugDepthTexture();
		// RenderToDebugColorTexture();
	}

	private void RenderToDebugDepthTexture()
	{
		if (_directionalLight == null)
		{
			return;
		}

		// GL.ClearColor(Color.Yellow.ToOtherColor());
		GL.Clear(ClearBufferMask.DepthBufferBit);

		DepthMapRenderTexture.Bind();
		GL.Viewport(0, 0, (int) DepthMapRenderTexture.Size.X, (int) DepthMapRenderTexture.Size.Y);
		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		// GL.ClearColor(1,0,1,1);

		DepthMapRenderTexture.RenderDepthAttachment(PassRenderTexture.DepthAttachment);

		DepthMapRenderTexture.Unbind();
	}
}