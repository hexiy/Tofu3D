using Tofu3D.Rendering;

namespace Tofu3D;

public class RenderPassDirectionalLightShadowDepth : RenderPass
{
	public static RenderPassDirectionalLightShadowDepth I { get; private set; }

	DirectionalLight _directionalLight;

	// shadows depth map
	public RenderTexture DebugGrayscaleTexture { get; private set; }

	public RenderPassDirectionalLightShadowDepth() : base(RenderPassType.DirectionalLightShadowDepth)
	{
		I = this;
	}

	public override bool CanRender()
	{
		return _directionalLight?.IsActive == true && Enabled;
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
		PassRenderTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: false, depthAttachment: true);
		PassRenderTexture.ClearColor = new Color(0, 150, 0, 255);
		DebugGrayscaleTexture = new RenderTexture(size: _directionalLight.Size, colorAttachment: true, depthAttachment: false);

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
		// bool renderToDebugTexture = GameObjectSelectionManager.GetSelectedGameObject()?.GetComponent<DirectionalLight>() != null;

		// if (renderToDebugTexture)
		// {
			RenderToDebugDepthTexture();
		// }
	}

	private void RenderToDebugDepthTexture()
	{
		if (_directionalLight == null)
		{
			return;
		}

		GL.ActiveTexture(TextureUnit.Texture1);

		DebugGrayscaleTexture.Bind();

		// GL.ClearColor(Color.Yellow.ToOtherColor());
		// GL.Clear(ClearBufferMask.ColorBufferBit);

		GL.Viewport(0, 0, (int) DebugGrayscaleTexture.Size.X, (int) DebugGrayscaleTexture.Size.Y);
		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		// GL.ClearColor(1,0,1,1);

		DebugGrayscaleTexture.RenderDepthAttachment(PassRenderTexture.DepthAttachment);

		DebugGrayscaleTexture.Unbind();
	}
}