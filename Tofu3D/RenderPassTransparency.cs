/*namespace Tofu3D.Rendering;

public class RenderPassTransparency : RenderPass
{
    public static RenderPassTransparency I { get; private set; }

    public RenderPassTransparency() : base(RenderPassType.Transparency)
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
            PassRenderTexture.Size = Camera.I.Size;
            PassRenderTexture.Invalidate(generateBrandNewTextures: false);
            return;
        }

        PassRenderTexture = new RenderTexture(size: Camera.I.Size, colorAttachment: true, depthAttachment: true);

        base.SetupRenderTexture();
    }
}*/

