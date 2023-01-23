namespace Scripts;

[Serializable]
public class Material
{
	public bool Additive = false;
	public string Path;
	public Shader Shader;

	public int Vao;
	public int Vbo;

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