namespace Tofu3D.Rendering;

public class RenderPassPostProcess : RenderPass
{
    private Material _postProcessMaterial;

    public RenderPassPostProcess() : base(RenderPassType.PostProcess)
    {
        I = this;
    }

    public static RenderPassPostProcess I { get; private set; }


    public override bool CanRender() => Enabled;

    public override void Initialize()
    {
        SetupRenderTexture();

        _postProcessMaterial = Tofu.AssetManager.Load<Material>("PostProcess");
        base.Initialize();
    }


    public override void RenderToRenderTexture(RenderTexture target, FramebufferAttachment attachment)
    {
        if (PassRenderTexture == null)
        {
            Debug.Log("PassRenderTexture == null");
            return;
        }


        target.Bind();


        Tofu.ShaderManager.UseShader(_postProcessMaterial.Shader);
        _postProcessMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
        _postProcessMaterial.Shader.SetFloat("u_time", Time.EditorElapsedTime);

        Tofu.ShaderManager.BindVertexArray(_postProcessMaterial.Vao);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(target.ColorAttachment);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        target.Unbind();
    }

    protected override void SetupRenderTexture()
    {
        if (PassRenderTexture != null)
        {
            PassRenderTexture.Size = Tofu.RenderPassSystem.ViewSize;
            PassRenderTexture.Invalidate(false);
            return;
        }

        PassRenderTexture = new RenderTexture(Tofu.RenderPassSystem.ViewSize, true, true);
    }
}