using System.IO;

namespace Scripts;

[Serializable]
public class Material
{
	public bool Additive = false;

	[XmlIgnore]
	public string FilePath
	{
		get { return _filePath; }
		private set { _filePath = value; }
	}
	private string _filePath;

	[XmlIgnore]
	public string FileName
	{
		get { return _fileName; }
		private set { _fileName = value; }
	}
	private string _fileName;

	public Shader Shader;

	public int Vao;
	public int Vbo;

	public void SetPath(string path)
	{
		FilePath = path;
		FileName = Path.GetFileName(FilePath);
	}

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