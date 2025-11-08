namespace TofuEngine.Rendering;

public class RenderPassBloomPostProcess : RenderPass
{
    private Asset_Material _bloomPostProcessMaterial;
    private Asset_Material _horizontalBlurMaterial;
    private Asset_Material _verticalBlurMaterial;

    private RenderPassBloomThreshold _renderPassBloomThreshold;
    public Framebuffer BloomFramebufferHorizontal { get; protected set; }
    public Framebuffer BloomFramebufferVertical { get; protected set; }
    
    public RenderPassBloomPostProcess(RenderPassBloomThreshold renderPassBloomThreshold, RenderTargetPipeline pipeline)
        : base(RenderPassType
            .BloomPostProcess, pipeline)
    {
        _renderPassBloomThreshold = renderPassBloomThreshold;
    }


    public override bool DrawsToTheFinalColorFramebuffer => true;
    public override bool CanRender() => Enabled;

    public override void Initialize()
    {
        SetupRenderTexture();

        _bloomPostProcessMaterial = Tofu.AssetLoadManager.Get<Asset_Material>("Assets/Materials/BloomPostProcess.mat");
        _horizontalBlurMaterial = Tofu.AssetLoadManager.Get<Asset_Material>("Assets/Materials/BloomHorizontal.mat");
        _verticalBlurMaterial = Tofu.AssetLoadManager.Get<Asset_Material>("Assets/Materials/BloomVertical.mat");
        base.Initialize();
    }


    public override void RenderThisAsFullscreenQuadToTargetFramebuffer(Framebuffer target,
        FramebufferAttachment attachment)
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

        Tofu.ShaderManager.BindVertexArray(Tofu.BasicMeshesCollection.RenderTextureMesh.Vao);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.One); // Additive blending for bloom effect
        GL.Viewport(0, 0, (int)target.Size.X, (int)target.Size.Y);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(BloomFramebufferHorizontal.TextureId);

        GL.ActiveTexture(TextureUnit.Texture1);
        TextureHelper.BindTexture(BloomFramebufferVertical
            .TextureId); // bind our threshold texture so we can combine them

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
        _horizontalBlurMaterial.Shader.SetFloat("texelWidth", 1f / BloomFramebufferHorizontal.Size.X);
        _horizontalBlurMaterial.Shader.SetFloat("texelHeight", 1f / BloomFramebufferHorizontal.Size.Y);
        GL.Viewport(0, 0, (int)BloomFramebufferHorizontal.Size.X, (int)BloomFramebufferHorizontal.Size.Y);

        Tofu.ShaderManager.BindVertexArray(Tofu.BasicMeshesCollection.RenderTextureMesh.Vao);

        GL.Disable(EnableCap.Blend);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(_renderPassBloomThreshold.MainFramebuffer
            .TextureId); // bind our existing screen texture

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
        _verticalBlurMaterial.Shader.SetFloat("texelWidth", 1f / BloomFramebufferVertical.Size.X);
        _verticalBlurMaterial.Shader.SetFloat("texelHeight", 1f / BloomFramebufferVertical.Size.Y);
        GL.Viewport(0, 0, (int)BloomFramebufferVertical.Size.X, (int)BloomFramebufferVertical.Size.Y);

        Tofu.ShaderManager.BindVertexArray(Tofu.BasicMeshesCollection.RenderTextureMesh.Vao);

        GL.Disable(EnableCap.Blend);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(BloomFramebufferHorizontal.TextureId); // bind our existing screen texture

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);

        BloomFramebufferVertical.Unbind();
    }

    protected override void Render_GL()
    {
    }

    protected override void SetupRenderTexture()
    {
        if (MainFramebuffer != null)
        {
            MainFramebuffer.Size = RenderTargetPipeline.FramebufferSize;
            MainFramebuffer.Invalidate(false);
        }
        else
        {
            MainFramebuffer = new Framebuffer(RenderTargetPipeline.FramebufferSize, true, false);
        }

        if (BloomFramebufferHorizontal != null)
        {
            BloomFramebufferHorizontal.Size = RenderTargetPipeline.FramebufferSize / 3f;
            BloomFramebufferHorizontal.Invalidate(false);
            BloomFramebufferVertical.Size = RenderTargetPipeline.FramebufferSize / 3f;
            BloomFramebufferVertical.Invalidate(false);
        }
        else
        {
            BloomFramebufferHorizontal =
                new Framebuffer(RenderTargetPipeline.FramebufferSize, true, false, downsampleFactor: 3);
            BloomFramebufferVertical =
                new Framebuffer(RenderTargetPipeline.FramebufferSize, true, false, downsampleFactor: 3);
        }
    }
}