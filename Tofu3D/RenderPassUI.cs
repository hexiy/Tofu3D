namespace Tofu3D.Rendering;

public class RenderPassUI : RenderPass
{
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

    protected override void SetupRenderTexture()
    {
        if (FinalFramebuffer != null)
        {
            FinalFramebuffer.Size = Tofu.RenderPassSystem.ViewSize;
            FinalFramebuffer.Invalidate(false);
            return;
        }

        FinalFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, true, true);
    }
}