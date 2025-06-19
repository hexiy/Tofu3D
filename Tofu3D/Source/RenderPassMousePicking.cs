namespace TofuEngine.Rendering;

public class RenderPassMousePicking : RenderPass
{
    public static RenderPassMousePicking I { get; private set; }
    public override bool DrawsToTheFinalColorFramebuffer => false;


    public override bool CanRender() =>
        RenderTargetPipeline.EditorPanelView.IsPanelHovered
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

        MainFramebuffer = new Framebuffer(RenderTargetPipeline.ViewSize, true, false, isIntegerFramebuffer: true);
    }

    protected override void PostRender()
    {
        Debug.StartTimer("Mouse picking pass time");
        MousePickingSystem.ReadPixelAtMousePos();
        Debug.EndAndStatTimer("Mouse picking pass time");
        base.PostRender();
    }
}