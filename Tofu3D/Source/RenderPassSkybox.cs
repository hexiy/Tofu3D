using TofuEngine.Rendering;

namespace TofuEngine;

public class RenderPassSkybox : RenderPass
{
    public override bool DrawsToTheFinalColorFramebuffer => true;
    private Skybox _skybox;

    public RenderPassSkybox(RenderTargetPipeline pipeline) : base(RenderPassType.Skybox, pipeline)
    {
    }

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
            _skybox = Camera.CurrentlyRenderingCamera.GetComponent<Skybox>();
        }

        if (_skybox == null)
        {
            return;
        }

        _skybox.RenderSkybox();
    }

    protected override void SetupRenderTexture()
    {
        MainFramebuffer = new Framebuffer(RenderTargetPipeline.FramebufferSize, true);
    }
}