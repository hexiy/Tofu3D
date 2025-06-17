using TofuEngine.Rendering;

namespace TofuEngine;

public class Framebuffer : ITexture
{
    // private readonly Asset_Material _depthRenderTextureMaterial;

    private readonly bool _hasColorAttachment;
    private readonly bool _hasDepthAttachment;
    private readonly bool _hasStencil;
    public Vector2 Size { get; set; }
    public int TextureId { get; set; }
    public int DepthTextureId = -1;

    private bool _isGrayscale;
    private readonly Asset_Material _renderTextureMaterial;
    public Color ClearColor = new Color(0, 0, 0, 0);
    public int FrameBufferID;

    // public Material RenderTextureMaterial;
    private int DownsampleFactor = 1;
    private readonly bool _isIntegerFramebuffer;
    private readonly bool _isCubemapDepth;

    public Framebuffer(Vector2 size, bool colorAttachment = false, bool depthAttachment = false,
        bool hasStencil = false, bool isGrayscale = false, int downsampleFactor = 1, bool isIntegerFramebuffer = false,
        bool isCubemapDepth = false)
    {
        // _depthRenderTextureMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/DepthRenderTexture.mat");
        // _renderTextureMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/RenderTexture.mat");

        // creating this in the library not assets, we need it as asset to reuse across other framebuffers and the asset system, but dont need to expose it to the user
        _renderTextureMaterial =
            Tofu.AssetLoadManager.Get<Asset_Material>(TofuPath.Combine(Folders.MaterialsInLibrary,
                "RenderTexture.mat.tofumaterial"));
        _renderTextureMaterial.Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.EngineResourcesShaders,"RenderTexture.glsl"));
        // _renderTextureMaterial = new Asset_Material()
        // { Shader = new Shader("Assets/Shaders/RenderTexture.glsl") };
        _renderTextureMaterial.LoadShader();

        // _depthRenderTextureMaterial = new Asset_Material()
        // { Shader = new Shader("Assets/Shaders/RenderTexture.glsl") };
        // _depthRenderTextureMaterial.LoadShader();
        DownsampleFactor = downsampleFactor;
        Size = size / downsampleFactor;
        _hasColorAttachment = colorAttachment;
        _hasDepthAttachment = depthAttachment;
        _hasStencil = hasStencil;
        _isGrayscale = isGrayscale;
        _isIntegerFramebuffer = isIntegerFramebuffer;
        _isCubemapDepth = isCubemapDepth;
        //GL.DeleteFramebuffers(1, ref id);
        // CreateMaterial();

        Invalidate();
    }

    /*void CreateMaterial()
    {
        RenderTextureMaterial = new Material();
        Shader shader = new(TofuPath.Combine(Folders.Shaders, "RenderTexture.glsl"));
        RenderTextureMaterial.SetShader(shader);
    }*/

    public void Invalidate(bool generateBrandNewTextures = true)
    {
        if (generateBrandNewTextures)
        {
            FrameBufferID = GL.GenFramebuffer();
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferID);

        if (_hasColorAttachment)
        {
            if (generateBrandNewTextures)
            {
                TextureId = GL.GenTexture();
            }

            //GL.CreateTextures(TextureTarget.Texture2D, 1, out colorAttachment);
            GL.BindTexture(TextureTarget.Texture2D, TextureId);

            // if (_isGrayscale)
            // {
            // 	GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R16, (int) Size.X, (int) Size.Y, 0, PixelFormat.Red, PixelType.UnsignedByte, (IntPtr) null);
            // }
            // else
            // {
            if (_isIntegerFramebuffer)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32ui, (int)Size.X, (int)Size.Y, 0,
                    PixelFormat.RedInteger, PixelType.UnsignedInt, (IntPtr)null);
            }
            else
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, (int)Size.X, (int)Size.Y, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
            }

            // }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, TextureId, 0);
        }

        if (_hasDepthAttachment)
        {
            if (generateBrandNewTextures)
            {
                DepthTextureId = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, DepthTextureId);

            // if (_isCubemapDepth)
            // {
            //     TextureHelper.BindTexture(DepthTextureId, TextureType.Cubemap);
            //
            //     for (var faceIndex = 0; faceIndex < 6; faceIndex++)
            //     {
            //         GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + faceIndex, 0, PixelInternalFormat.Rgba,
            //             Size.Xi, Size.Yi, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            //
            //         var textureTarget = TextureTarget.TextureCubeMap;
            //         GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //         GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            //         GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            //     }
            // }

            if (_hasStencil)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, (int)Size.X, (int)Size.Y,
                    0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, (IntPtr)null);
            }
            else
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, (int)Size.X, (int)Size.Y,
                    0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
            }

            TextureFilterMode textureFilterMode = TextureFilterMode.Bilinear;
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)textureFilterMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)textureFilterMode);


            // float[] borderColor =
            // {
            // 1.0f, 1.0f, 1.0f, 1.0f
            // };
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                _hasStencil ? FramebufferAttachment.DepthStencilAttachment : FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D, DepthTextureId, 0);


            // // MIPS
            // {
            //     GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)textureFilterMode);
            //     GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            //         (int)TextureMinFilter.LinearMipmapLinear);
            //     GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            //     int maxMipLevels = (int)Math.Floor(Math.Log2(Size.X));
            //     GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel,
            //         maxMipLevels); // Generate mipmaps for the cubemap texture
            // }

            if (_hasColorAttachment == false)
            {
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
            }
        }


        // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToBorder);
        // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToBorder);
        // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToBorder);
        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            Debug.Log("RENDER TEXTURE ERROR");
        }

        TofuGL.CheckGlError("Framebuffer invalidate error");

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferID);
    }

    public void Unbind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Clear()
    {
        Bind();
        GL.Viewport(0, 0, (int)Size.X, (int)Size.Y);

        // GL.ClearColor(ClearColor.ToOtherColor());
        GL.ClearColor(new Vector4(0, 0, 0, 0).ToColor().ToOtherColor());
        // GL.StencilMask(0xFF);
        // GL.Enable(EnableCap.StencilTest);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        Unbind();
    }

    public void RenderDepthAttachmentToThis(int texture)
    {
        Tofu.ShaderManager.UseShader(_renderTextureMaterial.Shader);
        _renderTextureMaterial.Shader.SetMatrix4X4("u_mvp",
            Matrix4x4.Identity); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

        Tofu.ShaderManager.BindVertexArray(Tofu.BasicMeshesCollection.RenderTextureMesh.Vao);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ActiveTexture(TextureUnit.Texture0);

        TextureHelper.BindTexture(texture);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);
    }

    public void RenderColorAttachmentToThis(int texture, BlendMode blendMode)
    {
        // return;
        // GL.Viewport(0, 0, (int) Size.X, (int) Size.Y);

        Tofu.ShaderManager.UseShader(_renderTextureMaterial.Shader);
        _renderTextureMaterial.Shader.SetMatrix4X4("u_mvp",
            Matrix4x4.Identity); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

        Tofu.ShaderManager.BindVertexArray(Tofu.BasicMeshesCollection.RenderTextureMesh.Vao);

        // GL.Enable(EnableCap.Blend);
        // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        RenderingBlendingHelper.SetBlendMode(blendMode);
        // RenderingBlendingHelper.SetBlendMode(BlendMode.Fade);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(texture);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);
    }
}