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
    public override void RenderToRenderTexture(RenderTexture target, FramebufferAttachment attachment)
    {
        if (PassRenderTexture == null)
        {
            Debug.Log("PassRenderTexture == null");
            return;
        }

        if (_bloomThresholdMaterial?.Shader == null)
        {
            Debug.Log("no bloom threshold material/shader");
            return;
        }


        PassRenderTexture.Bind();

        Tofu.ShaderManager.UseShader(_bloomThresholdMaterial.Shader);
        _bloomThresholdMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);

        Tofu.ShaderManager.BindVertexArray(_bloomThresholdMaterial.Vao);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(target.ColorAttachmentID); // bind our final texture(opaques)

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        PassRenderTexture.Unbind();
    }

    protected override void SetupRenderTexture()
    {
        if (PassRenderTexture != null)
        {
            PassRenderTexture.Size = Tofu.RenderPassSystem.ViewSize/4f;
            PassRenderTexture.Invalidate(false);
            return;
        }

        PassRenderTexture = new RenderTexture(Tofu.RenderPassSystem.ViewSize, true, false, downsampleFactor: 4);
    }
}