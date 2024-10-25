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
        GL.Enable(EnableCap.DepthTest);
        
        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderPassZPrePass.I.FinalFramebuffer.FrameBufferID);
        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FinalFramebuffer.FrameBufferID);
        var sizeX = (int)FinalFramebuffer.Size.X;
        var sizeY = (int)FinalFramebuffer.Size.Y;
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
        if (FinalFramebuffer != null)
        {
            FinalFramebuffer.Size = Tofu.RenderPassSystem.ViewSize;
            FinalFramebuffer.Invalidate(false);
            return;
        }

        FinalFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, true);
    }
}