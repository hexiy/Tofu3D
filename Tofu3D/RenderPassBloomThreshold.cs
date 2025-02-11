namespace Tofu3D.Rendering;

public class RenderPassBloomThreshold : RenderPass
{
    private Asset_Material _bloomThresholdMaterial;
    public static RenderPassBloomThreshold I;

    public RenderPassBloomThreshold() : base(RenderPassType.BloomThreshold)
    {
        I = this;
    }


    public override bool DrawsToTheFinalColorFramebuffer => false;
    public override bool CanRender() => Enabled;

    public override void Initialize()
    {
        SetupRenderTexture();

        _bloomThresholdMaterial = Tofu.AssetLoadManager.Get<Asset_Material>("Assets/Materials/BloomThreshold.mat");
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
        Tofu.ShaderManager.BindVertexArray(Tofu.BasicMeshesCollection.RenderTextureMesh.Vao);

        // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
        GL.Viewport(0,0, (int)MainFramebuffer.Size.X,(int)MainFramebuffer.Size.Y);
        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(target.TextureId); // bind our final texture(opaques)

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        MainFramebuffer.Unbind();
    }

    protected override void Render_GL()
    {
        
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = Tofu.RenderPassSystem.ViewSize/3;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize/3, true, false, downsampleFactor: 1);
    }
}