namespace TofuEngine.Rendering;

public class RenderPassTransparency : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => true;
    public override BlendMode BlendMode { get; } = BlendMode.Fade;

    public RenderPassTransparency(RenderTargetPipeline pipeline) : base(RenderPassType.Transparency, pipeline)
    {
    }

    public override void Initialize()
    {
        SetupRenderTexture();

        base.Initialize();
    }

    protected override void Render_GL()
    {
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.Transparent);
    }

    protected override void PreBindFrameBuffer()
    {
        GL.Enable(EnableCap.Blend);
        // GL.Enable(EnableCap.DepthTest);

        // blit depth(opaques only drawn in depth pre pass) so we dont draw transparent objects over everything
        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer,
            RenderTargetPipeline.ZPrePass.MainFramebuffer.FrameBufferID);
        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, MainFramebuffer.FrameBufferID);
        int sizeX = (int)MainFramebuffer.Size.X;
        int sizeY = (int)MainFramebuffer.Size.Y;
        GL.BlitFramebuffer(0, 0, sizeX, sizeY, 0, 0, sizeX, sizeY, ClearBufferMask.DepthBufferBit,
            BlitFramebufferFilter.Nearest);

        // blit skybox to this
        // GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderPassSkybox.I.MainFramebuffer.FrameBufferID);
        // GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, MainFramebuffer.FrameBufferID);
        // GL.BlitFramebuffer(0, 0, sizeX, sizeY, 0, 0, sizeX, sizeY, ClearBufferMask.ColorBufferBit,
        //     BlitFramebufferFilter.Nearest);


        base.PreBindFrameBuffer();
    }

    protected override void PreRender()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(false);  // Transparent objects don't write depth
        GL.DepthFunc(DepthFunction.Lequal);  // But still test against existing depth
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = RenderTargetPipeline.FramebufferSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(RenderTargetPipeline.FramebufferSize, true, true);
    }
}