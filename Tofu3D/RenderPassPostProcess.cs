namespace Tofu3D.Rendering;

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

		Material postProcessMaterial = AssetManager.Load<Material>("PostProcess");
		ShaderCache.UseShader(postProcessMaterial.Shader);
		postProcessMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
		postProcessMaterial.Shader.SetFloat("u_time", Time.EditorElapsedTime);

		ShaderCache.BindVertexArray(postProcessMaterial.Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.ActiveTexture(TextureUnit.Texture0);
		TextureHelper.BindTexture(target.ColorAttachment);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		DebugHelper.LogDrawCall();
		ShaderCache.BindVertexArray(0);

		target.Unbind();
	}

	protected override void SetupRenderTexture()
	{
		if (PassRenderTexture != null)
		{
			PassRenderTexture.Size = RenderPassSystem.ViewSize;
			PassRenderTexture.Invalidate(generateBrandNewTextures: false);
			return;
		}

		PassRenderTexture = new RenderTexture(size: RenderPassSystem.ViewSize, colorAttachment: true, depthAttachment: true);

		base.SetupRenderTexture();
	}
}