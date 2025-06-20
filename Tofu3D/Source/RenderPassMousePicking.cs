namespace TofuEngine.Rendering;

public class RenderPassMousePicking : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => false;


    public override bool CanRender() =>
        RenderTargetPipeline.EditorPanelView.IsPanelHovered
        //&& Tofu.MouseInput.IsButtonDown()
        && base.CanRender();
// make sure to check if mouse is in current scene view, not just any scene view

    public RenderPassMousePicking(RenderTargetPipeline pipeline) : base(RenderPassType.MousePicking, pipeline)
    {
    }


    public override void Initialize()
    {
        SetupRenderTexture();

        base.Initialize();
    }

    protected override void PreRender()
    {
        // Clear the framebuffer with 0 (no object)

        GL.ClearColor(0, 0, 0, 255);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        base.PreRender();
    }

    protected override void Render_GL()
    {
        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.All);
    }
    protected override void PostRender()
    {
        Debug.StartTimer("Mouse picking pass time");
        MousePickingSystem.ReadPixelAtMousePos(this);
        Debug.EndAndStatTimer("Mouse picking pass time");

        base.PostRender();
    }
    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = RenderTargetPipeline.FramebufferSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(RenderTargetPipeline.FramebufferSize, true, false, isIntegerFramebuffer: false);
    }


}