namespace TofuEngine.Rendering;

public class RenderPassOpaques : RenderPass
{
    public RenderPassOpaques(RenderTargetPipeline pipeline) : base(RenderPassType.Opaques, pipeline)
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

        if (RenderTargetPipeline.ZPrePass.Enabled)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer,
                RenderTargetPipeline.ZPrePass.MainFramebuffer.FrameBufferID);
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
        GL.Enable(EnableCap.DepthTest);

        if (RenderTargetPipeline.ZPrePass.Enabled)
        {
            GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Equal);
        }
        else
        {
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
        }
    }

    protected override void Render_GL()
    {
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.Opaque);
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