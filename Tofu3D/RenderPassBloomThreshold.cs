namespace Tofu3D.Rendering;

public class RenderPassBloomThreshold : RenderPass
{
    private Asset_Material _bloomThresholdMaterial;
    public static RenderPassBloomThreshold I;

    public RenderPassBloomThreshold() : base(RenderPassType.BloomThreshold)
    {
        I = this;
    }


    public override bool CanRender() => Enabled;

    public override void Initialize()
    {
        SetupRenderTexture();

        _bloomThresholdMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/BloomThreshold.mat");
        base.Initialize();
    }

    // this will not render to target(final) render texture, but our own
    public override void RenderThisAsFullscreenQuadToTargetFramebuffer(Framebuffer target,
        FramebufferAttachment attachment)
    {
        if (MainFramebuffer == null)
        {
            Debug.Log("PassRenderTexture == null");
            return;
        }

        if (_bloomThresholdMaterial?.Shader == null)
        {
            Debug.Log("no bloom threshold material/shader");
            return;
        }


        MainFramebuffer.Bind();

        Tofu.ShaderManager.UseShader(_bloomThresholdMaterial.Shader);
        _bloomThresholdMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
        _bloomThresholdMaterial.Shader.SetFloat("downsampleFactor", 4);
        Tofu.ShaderManager.BindVertexArray(_bloomThresholdMaterial.Vao);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(target.ColorAttachmentID); // bind our final texture(opaques)

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        MainFramebuffer.Unbind();
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = Tofu.RenderPassSystem.ViewSize / 4f;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, false, downsampleFactor: 4);
    }
}