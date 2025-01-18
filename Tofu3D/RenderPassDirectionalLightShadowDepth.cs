using Tofu3D.Rendering;

namespace Tofu3D;

public class RenderPassDirectionalLightShadowDepth : RenderPass
{
    private DirectionalLight _directionalLight;
    public override bool DrawsToTheFinalColorFramebuffer => false;

    public RenderPassDirectionalLightShadowDepth() : base(RenderPassType.DirectionalLightShadowDepth)
    {
        I = this;
    }

    public static RenderPassDirectionalLightShadowDepth I { get; private set; }

    // shadows depth map
    public Framebuffer DebugDepthVisualisationTexture { get; private set; }

    public override bool CanRender() => _directionalLight?.IsActive == true && Enabled;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetDirectionalLight(DirectionalLight directionalLight)
    {
        _directionalLight = directionalLight;

        SetupRenderTexture();
    }

    protected override void Render_GL()
    {
        if (_directionalLight == null)
        {
            return;
        }

        _directionalLight.RenderDirectionalLightShadowDepth();
    }

    protected override void SetupRenderTexture()
    {
        // PassRenderTexture contains the depth, we render that depth with DeptRenderTexture.glsl shader to DepthMapRenderTexture and use that as a shadowmap
        MainFramebuffer = new Framebuffer(_directionalLight.Size, false, true);
        MainFramebuffer.ClearColor = new Color(0, 150, 0, 255);
        DebugDepthVisualisationTexture = new Framebuffer(_directionalLight.Size, true);
    }

    protected override void PreRender()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(true);
GL.Clear(ClearBufferMask.DepthBufferBit);
        // it would be nice to render the skybox to the light view preview textures
        // RenderPassSkybox.I.Render();
        // RenderPassSkybox.I.RenderToRenderTexture(PassRenderTexture, FramebufferAttachment.Color);
    }

    protected override void PostRender()
    {
        base.PostRender();
        var renderToDebugTexture =
            Tofu.GameObjectSelectionManager.GetSelectedGameObject()?.GetComponent<DirectionalLight>() != null;

        if (renderToDebugTexture || true)
        {
            RenderToDebugDepthVisualisationTexture();
        }

        // GL.Disable(EnableCap.DepthTest);
    }

    private void RenderToDebugDepthVisualisationTexture()
    {
        if (_directionalLight == null)
        {
            return;
        }

        GL.ActiveTexture(TextureUnit.Texture1);

        DebugDepthVisualisationTexture.Bind();

        // GL.ClearColor(Color.Yellow.ToOtherColor());
        // GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.Viewport(0, 0, (int)DebugDepthVisualisationTexture.Size.X, (int)DebugDepthVisualisationTexture.Size.Y);
        // GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        // GL.ClearColor(1,0,1,1);

        DebugDepthVisualisationTexture.RenderDepthAttachmentToThis(MainFramebuffer.DepthTextureId);

        DebugDepthVisualisationTexture.Unbind();
    }
}