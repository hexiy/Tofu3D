namespace Tofu3D.Rendering;

public class RenderPassOpaques : RenderPass
{
    public RenderPassOpaques() : base(RenderPassType.Opaques)
    {
        I = this;
    }

    public static RenderPassOpaques I { get; private set; }

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
        var sizeX = (int)PassRenderTexture.Size.X;
        var sizeY = (int)PassRenderTexture.Size.Y;
        GL.BlitFramebuffer(0, 0, sizeX, sizeY, 0, 0, sizeX, sizeY, ClearBufferMask.DepthBufferBit,
            BlitFramebufferFilter.Nearest);

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
            PassRenderTexture.Size = Tofu.RenderPassSystem.ViewSize;
            PassRenderTexture.Invalidate(false);
            return;
        }

        PassRenderTexture = new RenderTexture(Tofu.RenderPassSystem.ViewSize, true, true);
    }
}