using Tofu3D.Rendering;

namespace Tofu3D;

public class RenderPassSkybox : RenderPass
{
    public RenderPassSkybox() : base(RenderPassType.Skybox)
    {
        I = this;
    }

    public static RenderPassSkybox I { get; private set; }
    // protected override bool CanRender()
    // {
    // 	return _directionalLight?.IsActive == true;
    // }

    public override void Initialize()
    {
        base.Initialize();
        SetupRenderTexture();
    }

    protected override void SetupRenderTexture()
    {
        PassRenderTexture = new RenderTexture(Tofu.RenderPassSystem.ViewSize, true);
    }
}