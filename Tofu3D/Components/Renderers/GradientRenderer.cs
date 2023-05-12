namespace Tofu3D;

public class GradientRenderer : Renderer
{
	[Show]
	public Color GradientColorA;
	[Show]
	public Color GradientColorB;

	public override void Awake()
	{
		base.Awake();

		SetDefaultMaterial();
	}

	public override void SetDefaultMaterial()
	{
		if (Material == null)
		{
			Material = AssetManager.Load<Material>("GradientMaterial");
		}

		base.SetDefaultMaterial();
	}

	public override void Render()
	{
		if (BoxShape == null || Material == null)
		{
			return;
		}

		ShaderManager.UseShader(Material.Shader);


		Material.Shader.SetVector4("u_color_a", GradientColorA.ToVector4());
		Material.Shader.SetVector4("u_color_b", GradientColorB.ToVector4());

		Material.Shader.SetVector2("u_resolution", Vector2.One);
		Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
		Material.Shader.SetColor("u_color", Color.ToVector4());
		Material.Shader.SetVector4("u_tint", (Vector4) Material.Shader.Uniforms["u_tint"]);
		if (Material.Shader.Uniforms.ContainsKey("time"))
		{
			Material.Shader.SetFloat("time", (float) Material.Shader.Uniforms["time"]);
		}

		ShaderManager.BindVertexArray(Material.Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		DebugHelper.LogDrawCall();
	}
}