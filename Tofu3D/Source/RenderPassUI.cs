namespace TofuEngine.Rendering;

public class RenderPassUI : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => true;

    public RenderPassUI() : base(RenderPassType.UI)
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
        
    }
    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = Tofu.RenderPassSystem.ViewSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, true, true);
    }
}