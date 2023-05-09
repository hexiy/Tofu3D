using System.IO;

namespace Scripts;

[Serializable]
public class Material : Asset<Material>
{
	[XmlIgnore]
	public int InstancedRenderingDefinitionIndex = -1;
	
	public bool Additive = false;

	public Shader Shader;

	public int Vao;
	public RenderMode RenderMode = RenderMode.Opaque;

	public void SetShader(Shader shader)
	{
		Shader = shader;

		Shader.Load();
		BufferFactory.CreateBufferForShader(this);
	}

	public void InitShader()
	{
		SetShader(Shader);
	}
}