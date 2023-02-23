using System.IO;

namespace Tofu3D;

[Serializable]
public class Shader : IDisposable
{
	public BufferType BufferType;

	public string Path;
	int _uLocationUColor = -1;

	int _uLocationUMvp = -1;

	[XmlIgnore]
	public Dictionary<string, object> Uniforms = new()
	                                             {
		                                             {"u_tint", new Vector4(1, 1, 1, 1)}
	                                             };

	public Shader()
	{
	}

	public Shader(string filePath)
	{
		Path = filePath;
	}

	[XmlIgnore] public int ProgramId { get; set; }

	public void Dispose()
	{
	}

	public void Load()
	{
		if (File.Exists(Path) == false)
		{
			Path = System.IO.Path.Combine("Assets", Path);
		}

		if (Path.Contains(".mat")) // IF ITS mat  not .glsl, just assign SpriteRenderer so we can fix it without crashing
		{
			Path = System.IO.Path.Combine("Assets", "Shaders", "SpriteRenderer.glsl");
		}

		if (File.Exists(Path) == false)
		{
			return;
		}
		GetAllUniforms();
		string shaderFile = File.ReadAllText(Path);

		string vertexCode = GetVertexShaderFromFileString(shaderFile);
		string fragmentCode = GetFragmentShaderFromFileString(shaderFile);
		BufferType = GetBufferTypeFromFileString(shaderFile);


		int vs, fs;

		vs = GL.CreateShader(ShaderType.VertexShaderArb);
		GL.ShaderSource(vs, vertexCode);
		GL.CompileShader(vs);

		string error = "";
		GL.GetShaderInfoLog(vs, out error);
		if (error.Length > 0)
		{
			System.Diagnostics.Debug.WriteLine("ERROR COMPILING VERTEX SHADER " + error);
		}

		fs = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(fs, fragmentCode);
		GL.CompileShader(fs);

		error = "";
		GL.GetShaderInfoLog(fs, out error);
		if (error.Length > 0)
		{
			System.Diagnostics.Debug.WriteLine("ERROR COMPILING FRAGMENT SHADER " + error);
		}

		ProgramId = GL.CreateProgram();
		GL.AttachShader(ProgramId, vs);
		GL.AttachShader(ProgramId, fs);

		GL.LinkProgram(ProgramId);

		// Delete shaders
		GL.DetachShader(ProgramId, vs);
		GL.DetachShader(ProgramId, fs);
		GL.DeleteShader(vs);
		GL.DeleteShader(fs);
	}

	public void SetMatrix4X4(string uniformName, Matrix4x4 mat)
	{
		// if (_uLocationUMvp == -1)
		// {
		// 	int location = GL.GetUniformLocation(ProgramId, uniformName);
		// 	_uLocationUMvp = location;
		// }

		int location = GL.GetUniformLocation(ProgramId, uniformName);

		GL.UniformMatrix4(location, 1, false, GetMatrix4X4Values(mat));
		Uniforms[uniformName] = mat;
	}

	public void SetFloat(string uniformName, float fl)
	{
		int location = GL.GetUniformLocation(ProgramId, uniformName);
		GL.Uniform1(location, fl);
		Uniforms[uniformName] = fl;
	}

	public void SetVector2(string uniformName, Vector2 vec)
	{
		int location = GL.GetUniformLocation(ProgramId, uniformName);
		GL.Uniform2(location, vec.X, vec.Y);
		Uniforms[uniformName] = vec;
	}

	public void SetVector3(string uniformName, Vector3 vec)
	{
		int location = GL.GetUniformLocation(ProgramId, uniformName);
		GL.Uniform3(location, vec.X, vec.Y, vec.Z);
		Uniforms[uniformName] = vec;
	}
	
	public void SetVector3Array(string uniformName, float[] floats)
	{
		int location = GL.GetUniformLocation(ProgramId, uniformName);
		GL.Uniform3(location, floats.Length, floats);
		Uniforms[uniformName] = floats;
	}

	public void SetFloatArray(string uniformName, float[] floats)
	{
		int location = GL.GetUniformLocation(ProgramId, uniformName);
		GL.Uniform1(location, floats.Length, floats);
		Uniforms[uniformName] = floats;
	}
	public void SetVector4(string uniformName, Vector4 vec)
	{
		int location = GL.GetUniformLocation(ProgramId, uniformName);
		GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
		Uniforms[uniformName] = vec;
	}

	public void SetColor(string uniformName, Color col)
	{
		SetColor(uniformName, col.ToVector4());
	}

	public void SetColor(string uniformName, Vector4 vec)
	{
		if (_uLocationUColor == -1)
		{
			int location = GL.GetUniformLocation(ProgramId, uniformName);
			_uLocationUColor = location;
		}

		GL.Uniform4(_uLocationUColor, vec.X, vec.Y, vec.Z, vec.W);
		Uniforms[uniformName] = vec;
	}

	// uniform sampler2D textureObject;
	// just find "uniform" and 2 words after; then we know the variables and we can display it 
	// todo
	public ShaderUniform[] GetAllUniforms()
	{
		List<ShaderUniform> uniforms = new();

		Path = Path.Replace(@"\", "/");
		string filename = System.IO.Path.GetFileName(Path);

		Path = System.IO.Path.Combine("Assets", "Shaders", filename);

		if (File.Exists(Path) == false)
		{
			return new ShaderUniform[] { };
		}

		using (StreamReader sr = new(Path))
		{
			string shaderString = sr.ReadToEnd();
			int currentIndexInString = 0;
			string trimmedShaderString = shaderString;

			while (trimmedShaderString.Contains("uniform"))
			{
				int startIndex = trimmedShaderString.IndexOf("uniform");
				int endIndex = startIndex + trimmedShaderString.Substring(startIndex).IndexOf(";");

				int endIndexWithEqualsOperator = startIndex + trimmedShaderString.Substring(startIndex).IndexOf("=");

				if (endIndexWithEqualsOperator < endIndex) // if we have "=", trim it so it isnt in the name
				{
					endIndex = endIndexWithEqualsOperator;
				}

				if (startIndex > endIndex)
				{
					break;
				}

				ShaderUniform uniform = new();

				string[] uniformString = trimmedShaderString.Substring(startIndex, endIndex - startIndex).Split(' ');

				uniform.Name = uniformString[2];
				uniform.Type = GetUniformType(uniformString[1]);
				currentIndexInString = endIndex + (shaderString.Length - trimmedShaderString.Length);
				trimmedShaderString = shaderString.Substring(currentIndexInString);

				uniforms.Add(uniform);
			}
		}

		return uniforms.ToArray();
	}

	Type GetUniformType(string typeName)
	{
		if (typeName == "vec4")
		{
			return typeof(Vector4);
		}

		if (typeName == "vec3")
		{
			return typeof(Vector3);
		}

		if (typeName == "mat4")
		{
			return typeof(Matrix4x4);
		}

		if (typeName == "float")
		{
			return typeof(float);
		}

		return typeof(string);
	}

	float[] GetMatrix4X4Values(Matrix4x4 m)
	{
		return new[]
		       {
			       m.M11, m.M12, m.M13, m.M14,
			       m.M21, m.M22, m.M23, m.M24,
			       m.M31, m.M32, m.M33, m.M34,
			       m.M41, m.M42, m.M43, m.M44
		       };
	}

	public int GetAttribLocation(string attribName)
	{
		return GL.GetAttribLocation(ProgramId, attribName);
	}

	public static BufferType GetBufferTypeFromFileString(string shaderFile)
	{
		string typeString = shaderFile.Substring(shaderFile.IndexOf("["),
		                                         shaderFile.IndexOf("]")); //File.ReadA;

		typeString = typeString.Substring(12);
		BufferType type;
		Enum.TryParse(typeString, out type);

		return type;
	}

	public static string GetVertexShaderFromFileString(string shaderFile)
	{
		return shaderFile.Substring(shaderFile.IndexOf("[VERTEX]") + 8,
		                            shaderFile.IndexOf("[FRAGMENT]") - shaderFile.IndexOf("[VERTEX]") - 8); //File.ReadA;
	}

	public static string GetFragmentShaderFromFileString(string shaderFile)
	{
		return shaderFile.Substring(shaderFile.IndexOf("[FRAGMENT]") + 10); //File.ReadA;
	}
}