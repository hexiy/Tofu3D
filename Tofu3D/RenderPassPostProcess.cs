namespace Tofu3D.Rendering;

public class RenderPassPostProcess : RenderPass
{
    private Asset_Material _postProcessMaterial;

    public RenderPassPostProcess() : base(RenderPassType.PostProcess)
    {
        I = this;
    }

    public static RenderPassPostProcess I { get; private set; }


    public override bool CanRender() => Enabled;

    public override void Initialize()
    {
        SetupRenderTexture();

        _postProcessMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/PostProcess.mat");
        base.Initialize();
    }


    public override void RenderThisAsFullscreenQuadToTargetFramebuffer(Framebuffer target, FramebufferAttachment attachment)
    {
        if (MainFramebuffer == null)
        {
            Debug.Log("PassRenderTexture == null");
            return;
        }


        target.Bind();


        Tofu.ShaderManager.UseShader(_postProcessMaterial.Shader);
        _postProcessMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
        _postProcessMaterial.Shader.SetFloat("u_time", Time.EditorElapsedTime);

        Tofu.ShaderManager.BindVertexArray(Tofu.BasicMeshesCollection.RenderTextureMesh.Vao);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(target.TextureId);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        target.Unbind();
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = Tofu.RenderPassSystem.ViewSize;
            MainFramebuffer.Invalidate(false);
            return;
        }

        MainFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, true);
    }
}