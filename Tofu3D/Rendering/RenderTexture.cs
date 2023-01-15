using System.IO;
using Tofu3D.Components.Renderers;

namespace Tofu3D;

public class RenderTexture
{
	public int ColorAttachment;
	public int DepthAttachment;
	public int Id;

	bool _hasColorAttachment;
	bool _hasDepthAttachment;

	public Material RenderTextureMaterial;
	Vector2 _size;

	public RenderTexture(Vector2 size, bool colorAttachment = false, bool depthAttachment = false)
	{
		_size = size;
		_hasColorAttachment = colorAttachment;
		_hasDepthAttachment = depthAttachment;
		//GL.DeleteFramebuffers(1, ref id);
		CreateMaterial();
		Invalidate(size);
	}

	void CreateMaterial()
	{
		RenderTextureMaterial = new Material();
		Shader shader = new(Path.Combine(Folders.Shaders, "RenderTexture.glsl"));
		RenderTextureMaterial.SetShader(shader);
	}

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

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, (int) size.X, (int) size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr) null);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Nearest);

			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorAttachment, 0);
		}

		if (_hasDepthAttachment)
		{
			DepthAttachment = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, DepthAttachment);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, (int) size.X, (int) size.Y, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, (IntPtr) null);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Nearest);

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

	public void RenderDepthAttachmentFullScreen(int targetTexture)
	{
		Material material = MaterialCache.GetMaterial("DepthRenderTexture");
		ShaderCache.UseShader(material.Shader);
		material.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity * Matrix4x4.CreateScale(1.8f)); //Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

		ShaderCache.BindVertexArray(material.Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		Debug.CountStat("Draw Calls", 1);
		ShaderCache.BindVertexArray(0);
	}

	/*public void RenderSnow(int targetTexture)
	{
		ShaderCache.UseShader(ShaderCache.snowShader);
		ShaderCache.snowShader.SetFloat("time", Time.elapsedTime * 0.08f);
		ShaderCache.snowShader.SetMatrix4x4("u_mvp", GetModelViewProjection(1));
		ShaderCache.snowShader.SetVector2("u_resolution", Camera.I.size);

		BufferCache.BindVAO(BufferCache.snowVAO);
		GL.Enable(EnableCap.Blend);

		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Lequal);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);

		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		GL.BlendFunc(BlendingFactor.SrcColor, BlendingFactor.One);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		BufferCache.BindVAO(0);
		GL.Disable(EnableCap.Blend);
		GL.Disable(EnableCap.DepthTest);
	}*/

	/*public void RenderWithPostProcess(int targetTexture)
	{
		ShaderCache.UseShader(ShaderCache.renderTexturePostProcessShader);
		ShaderCache.renderTexturePostProcessShader.SetVector2("u_resolution", Camera.I.size);
		ShaderCache.renderTexturePostProcessShader.SetMatrix4x4("u_mvp", GetModelViewProjection(1));

		BufferCache.BindVAO(BufferCache.renderTexturePostProcessVAO);
		GL.Enable(EnableCap.Blend);


		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		BufferCache.BindVAO(0);
		GL.Disable(EnableCap.Blend);
	}

	/*public void RenderBloom(int targetTexture, float sampleSize = 1)
	{
		ShaderCache.UseShader(ShaderCache.renderTextureBloomShader);
		ShaderCache.renderTextureBloomShader.SetVector2("u_resolution", Camera.I.size);
		ShaderCache.renderTextureBloomShader.SetMatrix4x4("u_mvp", GetModelViewProjection(sampleSize));

		BufferCache.BindVAO(BufferCache.renderTextureBloomVAO);
		GL.Enable(EnableCap.Blend);


		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstColor);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);

		TextureCache.BindTexture(targetTexture);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		BufferCache.BindVAO(0);
		GL.Disable(EnableCap.Blend);
	}*/

	public Matrix4x4 GetModelViewProjection(float sampleSize)
	{
		return Matrix4x4.CreateScale(2 * sampleSize, 2 * sampleSize, 1);
	}
}