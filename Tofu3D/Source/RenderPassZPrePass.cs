namespace TofuEngine.Rendering;

public class RenderPassZPrePass : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => false;

    public RenderPassZPrePass(RenderTargetPipeline pipeline) : base(RenderPassType.ZPrePass, pipeline)
    {
    }
    
    public override void Initialize()
    {
        SetupRenderTexture();

        base.Initialize();
    }
    protected override void Render_GL()
    {
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.Opaque);
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.Transparent);
    }
    protected override void PreRender()
    {
        GL.Enable(EnableCap.DepthTest); // Make sure depth test is enabled
        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Lequal);

        GL.ClearDepth(1);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.DepthRange(0, 1);
    }

    protected override void PostUnbindFrameBuffer()
    {
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = RenderTargetPipeline.FramebufferSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(RenderTargetPipeline.FramebufferSize, false, true);
    }
}