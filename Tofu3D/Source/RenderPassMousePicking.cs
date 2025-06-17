namespace TofuEngine.Rendering;

public class RenderPassMousePicking : RenderPass
{
    public static RenderPassMousePicking I { get; private set; }
    public override bool DrawsToTheFinalColorFramebuffer => false;

    
    public override bool CanRender() =>
        Tofu.MouseInput.IsMouseInView 
        //&& Tofu.MouseInput.IsButtonDown()
                                           && base.CanRender();
// make sure to check if mouse is in current scene view, not just any scene view

    public RenderPassMousePicking(RenderTargetPipeline pipeline) : base(RenderPassType.MousePicking, pipeline)
    {
        I = this;
    }


    public override void Initialize()
    {
        SetupRenderTexture();

        base.Initialize();
    }


    protected override void Render_GL()
    {
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.All);
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = RenderTargetPipeline.ViewSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(RenderTargetPipeline.ViewSize, true, false, isIntegerFramebuffer: false);
    }

    protected override void PreBindFrameBuffer()
    {
        // GL.Disable(EnableCap.DepthTest);

        // GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderPassZPrePass.I.MainFramebuffer.FrameBufferID);
        // GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, MainFramebuffer.FrameBufferID);
        // var sizeX = (int)MainFramebuffer.Size.X;
        // var sizeY = (int)MainFramebuffer.Size.Y;
        // GL.BlitFramebuffer(0, 0, sizeX, sizeY, 0, 0, sizeX, sizeY, ClearBufferMask.DepthBufferBit,
        //     BlitFramebufferFilter.Nearest);


        base.PreBindFrameBuffer();
    }

    protected override void PostRender()
    {
        Debug.StartTimer("Mouse picking pass time");
        MousePickingSystem.ReadPixelAtMousePos();
        Debug.EndAndStatTimer("Mouse picking pass time");
        base.PostRender();
    }

    // protected override void PostUnbindFrameBuffer()
    // {
    //     Debug.StartTimer("Mouse picking pass time");
    //     MousePickingSystem.ReadPixelAtMousePos();
    //     Debug.EndAndStatTimer("Mouse picking pass time");
    //     base.PostRender();
    // }
}