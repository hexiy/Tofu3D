﻿namespace Tofu3D;

public class RenderTexture
{
    private readonly Asset_Material _depthRenderTextureMaterial;

    private readonly bool _hasColorAttachment;
    private readonly bool _hasDepthAttachment;
    private readonly bool _hasStencil;

    private bool _isGrayscale;
    private readonly Asset_Material _renderTextureMaterial;
    public Color ClearColor = new(0, 0, 0, 0);
    public int ColorAttachmentID = -1;
    public int DepthAttachmentID = -1;
    public int FrameBufferID;

    // public Material RenderTextureMaterial;
    public Vector2 Size;
    private int DownsampleFactor=1;

    public RenderTexture(Vector2 size, bool colorAttachment = false, bool depthAttachment = false,
        bool hasStencil = false, bool isGrayscale = false, int downsampleFactor = 1)
    {
        _depthRenderTextureMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/DepthRenderTexture.mat");
        _renderTextureMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/RenderTexture.mat");
        DownsampleFactor = downsampleFactor;
        Size = size/downsampleFactor;
        _hasColorAttachment = colorAttachment;
        _hasDepthAttachment = depthAttachment;
        _hasStencil = hasStencil;
        _isGrayscale = isGrayscale;
        //GL.DeleteFramebuffers(1, ref id);
        // CreateMaterial();
        Invalidate();
    }

    /*void CreateMaterial()
    {
        RenderTextureMaterial = new Material();
        Shader shader = new(Path.Combine(Folders.Shaders, "RenderTexture.glsl"));
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
                ColorAttachmentID = GL.GenTexture();
            }

            //GL.CreateTextures(TextureTarget.Texture2D, 1, out colorAttachment);
            GL.BindTexture(TextureTarget.Texture2D, ColorAttachmentID);

            // if (_isGrayscale)
            // {
            // 	GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R16, (int) Size.X, (int) Size.Y, 0, PixelFormat.Red, PixelType.UnsignedByte, (IntPtr) null);
            // }
            // else
            // {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16, (int)Size.X, (int)Size.Y, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)null);
            // }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, ColorAttachmentID, 0);
        }

        if (_hasDepthAttachment)
        {
            if (generateBrandNewTextures)
            {
                DepthAttachmentID = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, DepthAttachmentID);

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

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);


            float[] borderColor =
            {
                1.0f, 1.0f, 1.0f, 1.0f
            };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToBorder);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                _hasStencil ? FramebufferAttachment.DepthStencilAttachment : FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D, DepthAttachmentID, 0);

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
        // GL.StencilMask(0xFF);
        // GL.Enable(EnableCap.StencilTest);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        Unbind();
    }

    public void RenderDepthAttachment(int texture)
    {
        Tofu.ShaderManager.UseShader(_depthRenderTextureMaterial.Shader);
        _depthRenderTextureMaterial.Shader.SetMatrix4X4("u_mvp",
            Matrix4x4.Identity); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

        Tofu.ShaderManager.BindVertexArray(_depthRenderTextureMaterial.Vao);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ActiveTexture(TextureUnit.Texture0);

        TextureHelper.BindTexture(texture);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);
    }

    public void RenderColorAttachment(int texture)
    {
        // return;
        // GL.Viewport(0, 0, (int) Size.X, (int) Size.Y);

        Tofu.ShaderManager.UseShader(_renderTextureMaterial.Shader);
        _renderTextureMaterial.Shader.SetMatrix4X4("u_mvp",
            Matrix4x4.Identity); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

        Tofu.ShaderManager.BindVertexArray(_renderTextureMaterial.Vao);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(texture);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        DebugHelper.LogDrawCall();
        Tofu.ShaderManager.BindVertexArray(0);
    }
}