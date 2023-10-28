namespace Tofu3D.Rendering;

public class RenderPassUI : RenderPass
{
    public static RenderPassUI I { get; private set; }

    public RenderPassUI() : base(RenderPassType.UI)
    {
        I = this;
    }

    public override void Initialize()
    {
        SetupRenderTexture();

        base.Initialize();
    }

    protected override void SetupRenderTexture()
    {
        if (PassRenderTexture != null)
        {
            PassRenderTexture.Size = Tofu.RenderPassSystem.ViewSize;
            PassRenderTexture.Invalidate(false);
            return;
        }

        PassRenderTexture = new RenderTexture(Tofu.RenderPassSystem.ViewSize, true, true, true);
    }
}