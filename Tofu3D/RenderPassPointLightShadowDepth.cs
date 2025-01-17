namespace Tofu3D.Rendering;

public class RenderPassPointLightShadowDepth : RenderPass
{
    public static RenderPassPointLightShadowDepth I { get; private set; }
    public override bool DrawsToTheFinalColorFramebuffer => false;

    public RenderPassPointLightShadowDepth() : base(RenderPassType.PointLightShadowDepth)
    {
        I = this;
    }

    public override bool CanRender() => Tofu.LightRenderingManager.PointLightsCount > 0 && Enabled;

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
        // PassRenderTexture contains the depth, we render that depth with DeptRenderTexture.glsl shader to DepthMapRenderTexture and use that as a shadowmap
        MainFramebuffer = new Framebuffer(new Vector2(2048, 2048), false, true, isCubemapDepth: true);
    }

    protected override void PreRender()
    {
        GL.DepthMask(true);
    }
}