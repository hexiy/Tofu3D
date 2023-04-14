﻿namespace Tofu3D.Rendering;

public class RenderPassPostProcess : RenderPass
{
	public static RenderPassPostProcess I { get; private set; }

	public RenderPassPostProcess() : base(RenderPassType.PostProcess)
	{
		I = this;
	}

	public override void Initialize()
	{
		SetupRenderTexture();

		base.Initialize();
	}

	int _lastFrameCleared = 0;
	public override void Clear()
	{
		if (Time.EditorElapsedTicks - _lastFrameCleared > 30)
		{
			//base.Clear();
		}
	}

	public override void RenderToFramebuffer(RenderTexture target, FramebufferAttachment attachment)
	{
		if (PassRenderTexture == null)
		{
			Debug.Log($"PassRenderTexture == null");
			return;
		}


		target.Bind();
		// if (attachment == FramebufferAttachment.Color)
		// {
		// 	target.RenderColorAttachment(PassRenderTexture.ColorAttachment);
		// }

		Material postProcessMaterial = MaterialCache.GetMaterial("PostProcess");
		ShaderCache.UseShader(postProcessMaterial.Shader);
		postProcessMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
		postProcessMaterial.Shader.SetFloat("u_time", Time.EditorElapsedTime);

		ShaderCache.BindVertexArray(postProcessMaterial.Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.ActiveTexture(TextureUnit.Texture0);
		TextureCache.BindTexture(target.ColorAttachment);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		Debug.StatAddValue("Draw Calls", 1);
		ShaderCache.BindVertexArray(0);

		target.Unbind();
	}

	protected override void SetupRenderTexture()
	{
		if (PassRenderTexture != null)
		{
			PassRenderTexture.Size = Camera.I.Size;
			PassRenderTexture.Invalidate(generateBrandNewTextures: false);
			return;
		}

		PassRenderTexture = new RenderTexture(size: Camera.I.Size, colorAttachment: true, depthAttachment: true);

		base.SetupRenderTexture();
	}
}