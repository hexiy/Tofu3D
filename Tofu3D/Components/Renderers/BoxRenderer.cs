namespace Tofu3D;

public class BoxRenderer : Renderer
{
	public override void Awake()
	{
		base.Awake();
	}

	public override void SetDefaultMaterial()
	{
		if (Material == null)
		{
			
			Material = Tofu.AssetManager.Load<Material>("BoxMaterial");
		}

		base.SetDefaultMaterial();
	}

	public override void Render()
	{
		if (BoxShape == null || Material == null)
		{
			return;
		}

		Tofu.ShaderManager.UseShader(Material.Shader);

		Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
		Material.Shader.SetColor("u_color", Color.ToVector4());
		//material.shader.SetVector4("u_tint", (Vector4) material.shader.uniforms["u_tint"]);
		if (Material.Shader.Uniforms.ContainsKey("time"))
		{
			Material.Shader.SetFloat("time", (float) Material.Shader.Uniforms["time"]);
		}

		Tofu.ShaderManager.BindVertexArray(Material.Vao);

		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		//GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
		DebugHelper.LogDrawCall();
	}
}