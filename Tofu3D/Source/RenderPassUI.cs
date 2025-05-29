namespace TofuEngine.Rendering;

public class RenderPassUI : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => true;

    public RenderPassUI(RenderTargetPipeline pipeline) : base(RenderPassType.UI, pipeline)
    {
        I = this;
    }

    public static RenderPassUI I { get; private set; }

    public override void Initialize()
    {
        SetupRenderTexture();

        base.Initialize();
    }

    protected override void Render_GL()
    {
        // ConfigureCameraForUIRender();

        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.UI);

        // ConfigureCameraForSceneRender();
    }

    // private void ConfigureCameraForUIRender()
    // {
    // }
    //
    // private void ConfigureCameraForSceneRender()
    // {
    // }

    protected override void PreRender()
    {
        base.PreRender();
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = RenderTargetPipeline.ViewSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(RenderTargetPipeline.ViewSize, true, true, true);
    }
}