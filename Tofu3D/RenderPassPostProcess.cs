namespace Tofu3D.Rendering;

public class RenderPassPostProcess : RenderPass
{
	public static RenderPassPostProcess I { get; private set; }
	Material _postProcessMaterial;


	public override bool CanRender()
	{
		return Enabled;
	}

	public RenderPassPostProcess() : base(RenderPassType.PostProcess)
	{
		I = this;
	}

	public override void Initialize()
	{
		SetupRenderTexture();

		_postProcessMaterial = AssetManager.Load<Material>("PostProcess");
		base.Initialize();
	}


	public override void RenderToFramebuffer(RenderTexture target, FramebufferAttachment attachment)
	{
		if (PassRenderTexture == null)
		{
			Debug.Log($"PassRenderTexture == null");
			return;
		}


		target.Bind();


		ShaderManager.UseShader(_postProcessMaterial.Shader);
		_postProcessMaterial.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
		_postProcessMaterial.Shader.SetFloat("u_time", Time.EditorElapsedTime);

		ShaderManager.BindVertexArray(_postProcessMaterial.Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.ActiveTexture(TextureUnit.Texture0);
		TextureHelper.BindTexture(target.ColorAttachment);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		DebugHelper.LogDrawCall();
		ShaderManager.BindVertexArray(0);

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