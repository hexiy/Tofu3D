namespace TofuEngine.Rendering;

public class RenderPassMousePicking : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => false;


    public override bool CanRender() =>
        base.CanRender() && RenderTargetPipeline.EditorPanelView == EditorViewManager.CurrentlyHoveredView;
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
        // Ensure depth testing is enabled for correct nearest-surface selection
        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(true);  // Write depth during mouse picking
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Disable(EnableCap.Blend);
        // Clear the framebuffer with 0 (no object)
        // Clear to transparent background so "no object" reads as 0 in all channels
        GL.ClearColor(0, 0, 0, 0);
        GL.ClearDepth(1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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
            // Ensure depth attachment exists; recreate if missing
            if (MainFramebuffer.DepthTextureId == -1)
            {
                MainFramebuffer = new Framebuffer(RenderTargetPipeline.FramebufferSize, true, true, isIntegerFramebuffer: false);
            }
            else
            {
                MainFramebuffer.Invalidate(false);
            }
            return;
        }

        MainFramebuffer = new Framebuffer(RenderTargetPipeline.FramebufferSize, true, true, isIntegerFramebuffer: false);
    }


}