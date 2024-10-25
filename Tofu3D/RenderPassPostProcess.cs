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
        if (FinalFramebuffer == null)
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
        TextureHelper.BindTexture(target.ColorAttachmentID);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        target.Unbind();
    }

    protected override void SetupRenderTexture()
    {
        if (FinalFramebuffer != null)
        {
            FinalFramebuffer.Size = Tofu.RenderPassSystem.ViewSize;
            FinalFramebuffer.Invalidate(false);
            return;
        }

        FinalFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, true);
    }
}