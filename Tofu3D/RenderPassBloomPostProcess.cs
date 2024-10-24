namespace Tofu3D.Rendering;

public class RenderPassBloomPostProcess : RenderPass
{
    private Asset_Material _bloomPostProcessMaterial;
    private Asset_Material _horizontalBlurMaterial;
    private Asset_Material _verticalBlurMaterial;

    private RenderPassBloomThreshold _renderPassBloomThreshold;
    public RenderTexture BloomRenderTextureHorizontal { get; protected set; }
    public RenderTexture BloomRenderTextureVertical { get; protected set; }

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

    public override void RenderToRenderTexture(RenderTexture target, FramebufferAttachment attachment)
    {
        if (PassRenderTexture == null)
        {
            Debug.Log("PassRenderTexture == null");
            return;
        }


        if (_bloomPostProcessMaterial?.Shader == null)
        {
            Debug.Log("no bloom post process material/shader");
            return;
        }

        BlurHorizontal();
        BlurVertical();
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
        TextureHelper.BindTexture(BloomRenderTextureHorizontal.ColorAttachmentID);

        GL.ActiveTexture(TextureUnit.Texture1);
        TextureHelper.BindTexture(BloomRenderTextureVertical
            .ColorAttachmentID); // bind our threshold texture so we can combine them

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        target.Unbind();
    }

    float[] kernel= new float[25]{
        1f/273.0f,  4f/273.0f,  6f/273.0f,  4f/273.0f, 1f/273.0f,
        4f/273.0f, 16f/273.0f, 24f/273.0f, 16f/273.0f, 4f/273.0f,
        6f/273.0f, 24f/273.0f, 36f/273.0f, 24f/273.0f, 6f/273.0f,
        4f/273.0f, 16f/273.0f, 24f/273.0f, 16f/273.0f, 4f/273.0f,
        1f/273.0f,  4f/273.0f,  6f/273.0f,  4f/273.0f, 1f/273.0f
    };
    private void BlurHorizontal()
    {
        BloomRenderTextureHorizontal.Bind();


        Tofu.ShaderManager.UseShader(_horizontalBlurMaterial.Shader);
        _horizontalBlurMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
        _horizontalBlurMaterial.Shader.SetFloat("texelWidth", 1f / BloomRenderTextureHorizontal.Size.X);
        _horizontalBlurMaterial.Shader.SetFloat("texelHeight", 1f / BloomRenderTextureHorizontal.Size.Y);
        
  
        _horizontalBlurMaterial.Shader.SetFloatArray("kernel", kernel);

        Tofu.ShaderManager.BindVertexArray(_horizontalBlurMaterial.Vao);

        GL.Disable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(_renderPassBloomThreshold.PassRenderTexture
            .ColorAttachmentID); // bind our existing screen texture

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        BloomRenderTextureHorizontal.Unbind();
    }

    private void BlurVertical()
    {
        BloomRenderTextureVertical.Bind();


        Tofu.ShaderManager.UseShader(_verticalBlurMaterial.Shader);
        _verticalBlurMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
        _verticalBlurMaterial.Shader.SetFloat("texelWidth", 1f / BloomRenderTextureVertical.Size.X);
        _verticalBlurMaterial.Shader.SetFloat("texelHeight", 1f / BloomRenderTextureVertical.Size.Y);
        _verticalBlurMaterial.Shader.SetFloatArray("kernel", kernel);

        Tofu.ShaderManager.BindVertexArray(_verticalBlurMaterial.Vao);

        GL.Disable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(BloomRenderTextureHorizontal
            .ColorAttachmentID); // bind our existing screen texture

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        BloomRenderTextureVertical.Unbind();
    }

    protected override void SetupRenderTexture()
    {
        if (PassRenderTexture != null)
        {
            PassRenderTexture.Size = Tofu.RenderPassSystem.ViewSize;
            PassRenderTexture.Invalidate(false);
        }
        else
        {
            PassRenderTexture = new RenderTexture(Tofu.RenderPassSystem.ViewSize, true, false);
        }

        if (BloomRenderTextureHorizontal != null)
        {
            BloomRenderTextureHorizontal.Size = Tofu.RenderPassSystem.ViewSize / 4f;
            BloomRenderTextureHorizontal.Invalidate(false);
            BloomRenderTextureVertical.Size = Tofu.RenderPassSystem.ViewSize / 4f;
            BloomRenderTextureVertical.Invalidate(false);
        }
        else
        {
            BloomRenderTextureHorizontal =
                new RenderTexture(Tofu.RenderPassSystem.ViewSize, true, false, downsampleFactor: 4);
            BloomRenderTextureVertical =
                new RenderTexture(Tofu.RenderPassSystem.ViewSize, true, false, downsampleFactor: 4);
        }
    }
}