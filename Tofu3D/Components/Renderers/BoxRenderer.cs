namespace Tofu3D;

public class BoxRenderer : Renderer
{
	public override void Awake()
	{
		base.Awake();
	}

	public override void CreateMaterial()
	{
		if (Material == null)
		{
			Material = MaterialCache.GetMaterial("BoxMaterial");
		}

		base.CreateMaterial();
	}

	public override void Render()
	{
		return;
		if (BoxShape == null || Material == null)
		{
			return;
		}
		
		ShaderCache.UseShader(Material.Shader);

		Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
		Material.Shader.SetColor("u_color", Color.ToVector4());
		//material.shader.SetVector4("u_tint", (Vector4) material.shader.uniforms["u_tint"]);
		if (Material.Shader.Uniforms.ContainsKey("time"))
		{
			Material.Shader.SetFloat("time", (float) Material.Shader.Uniforms["time"]);
		}

		ShaderCache.BindVertexArray(Material.Vao);

		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		//GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
		Debug.CountStat("Draw Calls", 1);
	}
}