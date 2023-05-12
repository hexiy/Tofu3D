using System.Drawing;
using System.IO;

namespace Tofu3D;

public class RenderTexture
{
	public int ColorAttachment;
	public int DepthAttachment;
	public int Id;

	bool _hasColorAttachment;
	bool _hasDepthAttachment;
	bool _hasStencil;

	bool _isGrayscale;

	// public Material RenderTextureMaterial;
	public Vector2 Size;
	public Color ClearColor = new Color(0, 0, 0, 0);

	Material _depthRenderTextureMaterial;
	Material _renderTextureMaterial;

	public RenderTexture(Vector2 size, bool colorAttachment = false, bool depthAttachment = false, bool hasStencil = false, bool isGrayscale = false)
	{
		_depthRenderTextureMaterial = AssetManager.Load<Material>("DepthRenderTexture");
		_renderTextureMaterial = AssetManager.Load<Material>("RenderTexture");
		Size = size;
		_hasColorAttachment = colorAttachment;
		_hasDepthAttachment = depthAttachment;
		_hasStencil = hasStencil;
		_isGrayscale = isGrayscale;
		//GL.DeleteFramebuffers(1, ref id);
		// CreateMaterial();
		Invalidate(generateBrandNewTextures: true);
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
			Id = GL.GenFramebuffer();
		}

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, Id);

		if (_hasColorAttachment)
		{
			if (generateBrandNewTextures)
			{
				ColorAttachment = GL.GenTexture();
			}

			//GL.CreateTextures(TextureTarget.Texture2D, 1, out colorAttachment);
			GL.BindTexture(TextureTarget.Texture2D, ColorAttachment);

			// if (_isGrayscale)
			// {
			// 	GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R16, (int) Size.X, (int) Size.Y, 0, PixelFormat.Red, PixelType.UnsignedByte, (IntPtr) null);
			// }
			// else
			// {
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16, (int) Size.X, (int) Size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr) null);
			// }

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorAttachment, 0);
		}

		if (_hasDepthAttachment)
		{
			if (generateBrandNewTextures)
			{
				DepthAttachment = GL.GenTexture();
			}

			GL.BindTexture(TextureTarget.Texture2D, DepthAttachment);

			if (_hasStencil)
			{
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, (int) Size.X, (int) Size.Y, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, (IntPtr) null);
			}
			else
			{
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, (int) Size.X, (int) Size.Y, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, (IntPtr) null);
			}

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);


			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, _hasStencil ? FramebufferAttachment.DepthStencilAttachment : FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthAttachment, 0);

			if (_hasColorAttachment == false)
			{
				GL.DrawBuffer(DrawBufferMode.None);
				GL.ReadBuffer(ReadBufferMode.None);
			}
		}


		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToBorder);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToBorder);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToBorder);
		if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
		{
			Debug.Log("RENDER TEXTURE ERROR");
		}

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	public void Bind()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
	}

	public void Unbind()
	{
		GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}

	public void Clear()
	{
		Bind();
		GL.Viewport(0, 0, (int) Size.X, (int) Size.Y);
		
		// GL.ClearColor(ClearColor.ToOtherColor());
		// GL.StencilMask(0xFF);
		// GL.Enable(EnableCap.StencilTest);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		Unbind();
	}

	public void RenderDepthAttachment(int targetTexture)
	{
		ShaderManager.UseShader(_depthRenderTextureMaterial.Shader);
		_depthRenderTextureMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

		ShaderManager.BindVertexArray(_depthRenderTextureMaterial.Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		// GL.ActiveTexture(TextureUnit.Texture0);

		TextureHelper.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		DebugHelper.LogDrawCall();
		ShaderManager.BindVertexArray(0);
	}

	public void RenderColorAttachment(int targetTexture)
	{
		// return;
		// GL.Viewport(0, 0, (int) Size.X, (int) Size.Y);

		ShaderManager.UseShader(_renderTextureMaterial.Shader);
		_renderTextureMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

		ShaderManager.BindVertexArray(_renderTextureMaterial.Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.ActiveTexture(TextureUnit.Texture0);
		TextureHelper.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		DebugHelper.LogDrawCall();
		ShaderManager.BindVertexArray(0);
	}
}