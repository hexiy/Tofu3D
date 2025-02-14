using Tofu3D.Rendering;

namespace Tofu3D;

public class RenderPassSkybox : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => true;
    private Skybox _skybox;

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
        Scene.SceneDisposed += () => _skybox = null;
    }

    protected override void Render_GL()
    {
        if (_skybox == null)
        {
            _skybox = Camera.MainCamera.GetComponent<Skybox>();
        }

        if (_skybox == null)
        {
            return;
        }

        _skybox.RenderSkybox();
    }

    protected override void SetupRenderTexture()
    {
        MainFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true);
    }
}