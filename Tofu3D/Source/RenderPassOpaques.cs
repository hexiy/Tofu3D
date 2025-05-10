namespace TofuEngine.Rendering;

public class RenderPassOpaques : RenderPass
{
    public RenderPassOpaques() : base(RenderPassType.Opaques)
    {
        I = this;
    }
    public override bool DrawsToTheFinalColorFramebuffer => true;

    public static RenderPassOpaques I { get; private set; }

    public override void Initialize()
    {
        SetupRenderTexture();

        base.Initialize();
    }

    protected override void PreBindFrameBuffer()
    {
        // GL.Enable(EnableCap.DepthTest);

        if (RenderPassZPrePass.I.Enabled)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderPassZPrePass.I.MainFramebuffer.FrameBufferID);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, MainFramebuffer.FrameBufferID);
            int sizeX = (int)MainFramebuffer.Size.X;
            int sizeY = (int)MainFramebuffer.Size.Y;
            GL.BlitFramebuffer(0, 0, sizeX, sizeY, 0, 0, sizeX, sizeY, ClearBufferMask.DepthBufferBit,
                BlitFramebufferFilter.Nearest);
        }
        else
        {
            // GL.Clear(ClearBufferMask.DepthBufferBit);
        }


        // blit skybox to this
        // GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderPassSkybox.I.MainFramebuffer.FrameBufferID);
        // GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, MainFramebuffer.FrameBufferID);
        // GL.BlitFramebuffer(0, 0, sizeX, sizeY, 0, 0, sizeX, sizeY, ClearBufferMask.ColorBufferBit,
        //     BlitFramebufferFilter.Nearest);


        base.PreBindFrameBuffer();
    }

    protected override void PreRender()
    {
        GL.DepthMask(true);
    }

    protected override void Render_GL()
    {
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.Opaque);
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = Tofu.RenderingSystem.SceneViewPipeline.ViewSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(Tofu.RenderingSystem.SceneViewPipeline.ViewSize, true, true);
    }
}