using Tofu3D;

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
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = Tofu.RenderPassSystem.ViewSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, false, isIntegerFramebuffer: false);
    }

    protected override void PostRender()
    {
        Debug.StartTimer("Mouse picking pass time");
        if (Tofu.MouseInput.IsMouseInSceneView)
        {
            MousePickingSystem.ReadPixelAtMousePos();
        }


        Debug.EndAndStatTimer("Mouse picking pass time");
        base.PostRender();
    }
}