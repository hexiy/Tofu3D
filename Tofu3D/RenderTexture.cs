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

	// public Material RenderTextureMaterial;
	public Vector2 Size;

	Material _depthRenderTextureMaterial;
	Material _renderTextureMaterial;

	public RenderTexture(Vector2 size, bool colorAttachment = false, bool depthAttachment = false)
	{
		_depthRenderTextureMaterial = MaterialCache.GetMaterial("DepthRenderTexture");
		_renderTextureMaterial = MaterialCache.GetMaterial("RenderTexture");
		Size = size;
		_hasColorAttachment = colorAttachment;
		_hasDepthAttachment = depthAttachment;
		//GL.DeleteFramebuffers(1, ref id);
		// CreateMaterial();
		Invalidate(size);
	}

	/*void CreateMaterial()
	{
		RenderTextureMaterial = new Material();
		Shader shader = new(Path.Combine(Folders.Shaders, "RenderTexture.glsl"));
		RenderTextureMaterial.SetShader(shader);
	}*/

	public void Invalidate(Vector2 size)
	{
		Id = GL.GenFramebuffer();

		GL.BindFramebuffer(FramebufferTarget.Framebuffer, Id);

		if (_hasColorAttachment)
		{
			ColorAttachment = GL.GenTexture();
			//GL.CreateTextures(TextureTarget.Texture2D, 1, out colorAttachment);
			GL.BindTexture(TextureTarget.Texture2D, ColorAttachment);
			//TextureCache.BindTexture(colorAttachment);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16, (int) size.X, (int) size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr) null);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorAttachment, 0);
		}

		if (_hasDepthAttachment)
		{
			DepthAttachment = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, DepthAttachment);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, (int) size.X, (int) size.Y, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, (IntPtr) null);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);


			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthAttachment, 0);

			if (_hasColorAttachment == false)
			{
				GL.DrawBuffer(DrawBufferMode.None);
				GL.ReadBuffer(ReadBufferMode.None);
			}
		}

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
		GL.ClearColor(Color.Black.ToOtherColor());
		GL.ClearDepth(1);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		Unbind();
	}

	public void RenderDepthAttachment(int targetTexture)
	{
		ShaderCache.UseShader(_depthRenderTextureMaterial.Shader);
		_depthRenderTextureMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

		ShaderCache.BindVertexArray(_depthRenderTextureMaterial.Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		Debug.StatAddValue("Draw Calls", 1);
		ShaderCache.BindVertexArray(0);
	}

	public void RenderColorAttachment(int targetTexture)
	{
		// return;
		// GL.Viewport(0, 0, (int) Size.X, (int) Size.Y);


		ShaderCache.UseShader(_renderTextureMaterial.Shader);
		_renderTextureMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

		ShaderCache.BindVertexArray(_renderTextureMaterial.Vao);

		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 1);

		Debug.StatAddValue("Draw Calls", 1);
		ShaderCache.BindVertexArray(0);
	}
}