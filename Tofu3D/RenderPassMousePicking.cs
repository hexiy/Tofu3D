/*using Engine;

namespace Tofu3D.Rendering;

public class RenderPassMousePicking : RenderPass
{
    public static RenderPassMousePicking I { get; private set; }

    public RenderPassMousePicking() : base(RenderPassType.MousePicking)
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
        PassRenderTexture = new RenderTexture(size: Camera.I.Size, colorAttachment: true, depthAttachment: true);

        base.SetupRenderTexture();
    }

    protected override void PostRender()
    {
        Debug.StartTimer("Mouse picking");
        MousePickingSystem.ReadPixelAtMousePos();

        Debug.EndAndStatTimer("Mouse picking");
        base.PostRender();
    }
}*/

