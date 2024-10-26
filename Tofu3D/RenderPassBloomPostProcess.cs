namespace Tofu3D.Rendering;

public class RenderPassBloomPostProcess : RenderPass
{
    private Asset_Material _bloomPostProcessMaterial;
    private Asset_Material _horizontalBlurMaterial;
    private Asset_Material _verticalBlurMaterial;

    private RenderPassBloomThreshold _renderPassBloomThreshold;
    public Framebuffer BloomFramebufferHorizontal { get; protected set; }
    public Framebuffer BloomFramebufferVertical { get; protected set; }

    public static RenderPassBloomPostProcess I;

    public RenderPassBloomPostProcess(RenderPassBloomThreshold renderPassBloomThreshold) : base(RenderPassType
        .BloomPostProcess)
    {
        I = this;
        _renderPassBloomThreshold = renderPassBloomThreshold;
    }


    public override bool CanRender() => Enabled;

    public override void Initialize()
    {
        SetupRenderTexture();

        _bloomPostProcessMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/BloomPostProcess.mat");
        _horizontalBlurMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/BloomHorizontal.mat");
        _verticalBlurMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/BloomVertical.mat");
        base.Initialize();
    }

    private float _blurOffset = 0f;

    public override void RenderThisAsFullscreenQuadToTargetFramebuffer(Framebuffer target, FramebufferAttachment attachment)
    {
        if (MainFramebuffer == null)
        {
            Debug.Log("PassRenderTexture == null");
            return;
        }


        if (_bloomPostProcessMaterial?.Shader == null)
        {
            Debug.Log("no bloom post process material/shader");
            return;
        }

        // for (int i = 0; i < 10; i++)
        // {
        //     _blurOffset = i * 0.00006f;
        //     BlurHorizontal();
        //     BlurVertical();
        // }
        BlurHorizontal();
        BlurVertical();

        target.Bind();


        Tofu.ShaderManager.UseShader(_bloomPostProcessMaterial.Shader);
        _bloomPostProcessMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);

        Tofu.ShaderManager.BindVertexArray(_bloomPostProcessMaterial.Vao);

        // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.Blend);

        // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.One); // Additive blending for bloom effect
        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(BloomFramebufferHorizontal.ColorAttachmentID);

        GL.ActiveTexture(TextureUnit.Texture1);
        TextureHelper.BindTexture(BloomFramebufferVertical
            .ColorAttachmentID); // bind our threshold texture so we can combine them

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        target.Unbind();
    }

    private void BlurHorizontal()
    {
        BloomFramebufferHorizontal.Bind();


        Tofu.ShaderManager.UseShader(_horizontalBlurMaterial.Shader);
        _horizontalBlurMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
        _horizontalBlurMaterial.Shader.SetFloat("texelWidth", 1f / BloomFramebufferHorizontal.Size.X + _blurOffset);
        _horizontalBlurMaterial.Shader.SetFloat("texelHeight", 1f / BloomFramebufferHorizontal.Size.Y + _blurOffset);

        Tofu.ShaderManager.BindVertexArray(_horizontalBlurMaterial.Vao);

        GL.Disable(EnableCap.Blend);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(_renderPassBloomThreshold.MainFramebuffer
            .ColorAttachmentID); // bind our existing screen texture

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        BloomFramebufferHorizontal.Unbind();
    }

    private void BlurVertical()
    {
        BloomFramebufferVertical.Bind();


        Tofu.ShaderManager.UseShader(_verticalBlurMaterial.Shader);
        _verticalBlurMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
        _verticalBlurMaterial.Shader.SetFloat("texelWidth", 1f / BloomFramebufferVertical.Size.X + _blurOffset);
        _verticalBlurMaterial.Shader.SetFloat("texelHeight", 1f / BloomFramebufferVertical.Size.Y + _blurOffset);

        Tofu.ShaderManager.BindVertexArray(_verticalBlurMaterial.Vao);

        GL.Disable(EnableCap.Blend);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(BloomFramebufferHorizontal
            .ColorAttachmentID); // bind our existing screen texture

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        BloomFramebufferVertical.Unbind();
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = Tofu.RenderPassSystem.ViewSize;
            MainFramebuffer.Invalidate(false);
        }
        else
        {
            MainFramebuffer = new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, false);
        }

        if (BloomFramebufferHorizontal != null)
        {
            BloomFramebufferHorizontal.Size = Tofu.RenderPassSystem.ViewSize / 4f;
            BloomFramebufferHorizontal.Invalidate(false);
            BloomFramebufferVertical.Size = Tofu.RenderPassSystem.ViewSize / 4f;
            BloomFramebufferVertical.Invalidate(false);
        }
        else
        {
            BloomFramebufferHorizontal =
                new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, false, downsampleFactor: 4);
            BloomFramebufferVertical =
                new Framebuffer(Tofu.RenderPassSystem.ViewSize, true, false, downsampleFactor: 4);
        }
    }
}