namespace TofuEngine.Rendering;

public class RenderPassZPrePass : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => false;

    public RenderPassZPrePass(RenderTargetPipeline pipeline) : base(RenderPassType.ZPrePass, pipeline)
    {
        I = this;
    }

    public static RenderPassZPrePass I { get; private set; }

    public override void Initialize()
    {
        SetupRenderTexture();

        base.Initialize();
    }
    protected override void Render_GL()
    {
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.Opaque);
    }
    protected override void PreRender()
    {
        // GL.Enable(EnableCap.DepthTest);

        GL.DepthMask(true);

        GL.ClearDepth(1);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        // GL.DepthRange(0, Camera.MainCamera.FarPlaneDistance);
        GL.DepthRange(0, 1);
        GL.DepthFunc(DepthFunction.Lequal);
    }

    protected override void PostUnbindFrameBuffer()
    {
        GL.DepthMask(false);
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = RenderTargetPipeline.ViewSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(RenderTargetPipeline.ViewSize, false, true);
    }
}