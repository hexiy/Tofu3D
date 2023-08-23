namespace Tofu3D.Rendering;

public class RenderPassOpaques : RenderPass
{
	public static RenderPassOpaques I { get; private set; }

	public RenderPassOpaques() : base(RenderPassType.Opaques)
	{
		I = this;
	}

	public override void Initialize()
	{
		SetupRenderTexture();

		base.Initialize();
	}

	protected override void PreBindFrameBuffer()
	{

		// GL.DepthMask(true);
		GL.Enable(EnableCap.DepthTest);
		// GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

		GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderPassZPrePass.I.PassRenderTexture.Id);
		GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, PassRenderTexture.Id);
		int sizeX = (int) PassRenderTexture.Size.X;
		int sizeY = (int) PassRenderTexture.Size.Y;
		GL.BlitFramebuffer(0, 0, sizeX, sizeY, 0, 0, sizeX, sizeY, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
		
		base.PreBindFrameBuffer();
	}

	protected override void PreRender()
	{
		GL.DepthMask(false);
	}

	protected override void SetupRenderTexture()
	{
		if (PassRenderTexture != null)
		{
			PassRenderTexture.Size = Tofu.I.RenderPassSystem.ViewSize;
			PassRenderTexture.Invalidate(generateBrandNewTextures: false);
			return;
		}

		PassRenderTexture = new RenderTexture(size: Tofu.I.RenderPassSystem.ViewSize, colorAttachment: true, depthAttachment: true);
	}
}