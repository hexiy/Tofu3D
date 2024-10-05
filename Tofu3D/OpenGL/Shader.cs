using System.IO;

namespace Tofu3D;

[Serializable]
public class
    Shader : IDisposable
{
    private float[] _getMatrix4X4ValuesArray =
    {
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0
    };

    private int _uLocationUColor = -1;

    private int _uLocationUMvp = -1;

    public int AlbedoTextureLocation;
    public int NormalTextureLocation;
    public int AmbientOcclusionTextureLocation;

    public BufferType BufferType;

    public string Path;
    public int ShadowMapTextureLocation;

    [XmlIgnore] public Dictionary<string, object> Uniforms = new()
    {
        { "u_tint", new Vector4(1, 1, 1, 1) }
    };

    public Shader()
    {
    }

    public Shader(string filePath)
    {
        Path = filePath;
    }

    [XmlIgnore] public bool IsLoaded { get; private set; }

    [XmlIgnore] public int ProgramId { get; set; }

    public void Dispose()
    {
        // GL.DeleteProgram(ProgramId); // dont really do this since multiple materials can be using the shader
    }

    // make Uniforms List<ShaderUniform> and get index from that
    public int GetUniformLocation(string uniformName) => GL.GetUniformLocation(ProgramId, uniformName);

    public void Load()
    {
        AssetUtils.ValidateAssetPath(ref Path);

        if (AssetUtils.Exists(Path) == false)
        {
            var newPath = System.IO.Path.Combine("Assets", Path);
            if (AssetUtils.Exists(newPath))
            {
                Path = newPath;
            }
        }

        if (Path.Contains(
                ".mat")) // IF ITS mat  not .glsl, just assign SpriteRenderer so we can fix it without crashing
        {
            Path = System.IO.Path.Combine("Assets", "Shaders", "SpriteRenderer.glsl");
        }

        if (AssetUtils.Exists(Path) == false)
        {
            Debug.Log($"Couldn't find shader:{Path}");
            // throw new FileNotFoundException("Couldn't find shader");
            return;
        }

        // GetAllUniforms();
        var shaderFile = File.ReadAllText(Path);

        var vertexCode = GetVertexShaderFromFileString(shaderFile);
        var fragmentCode = GetFragmentShaderFromFileString(shaderFile);
        BufferType = GetBufferTypeFromFileString(shaderFile);


        int vs, fs;

        vs = GL.CreateShader(ShaderType.VertexShaderArb);
        GL.ShaderSource(vs, vertexCode);
        GL.CompileShader(vs);

        var error = "";
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


        Tofu.ShaderManager.UseShader(this);
        AlbedoTextureLocation = GetUniformLocation("textureAlbedo");
        NormalTextureLocation = GetUniformLocation("textureNormal");
        AmbientOcclusionTextureLocation = GetUniformLocation("textureAo");
        ShadowMapTextureLocation = GetUniformLocation("shadowMap");

        int indx = 0;
        if (AlbedoTextureLocation != -1)
        {
            GL.Uniform1(AlbedoTextureLocation, indx);
            indx++;
        }

        if (NormalTextureLocation != -1)
        {
            GL.Uniform1(NormalTextureLocation, indx);
            indx++;
        }

        if (AmbientOcclusionTextureLocation != -1)
        {
            GL.Uniform1(AmbientOcclusionTextureLocation, indx);
            indx++;
        }

        if (ShadowMapTextureLocation != -1)
        {
            GL.Uniform1(ShadowMapTextureLocation, indx);
            indx++;
        }

        // int mainTextureLocation = GL.GetUniformLocation(ShaderCache.ShaderInUse, "textureObject");
        // int shadowMapTextureLocation = GL.GetUniformLocation(ShaderCache.ShaderInUse, "shadowMap");
        // if (mainTextureLocation != -1)
        // {
        // 	GL.Uniform1(mainTextureLocation, 0);
        // }
        //
        // if (shadowMapTextureLocation != -1)
        // {
        // 	GL.Uniform1(shadowMapTextureLocation, 1);
        // }
        IsLoaded = true;
    }

    public void SetMatrix4X4(string uniformName, Matrix4x4 mat)
    {
        // if (_uLocationUMvp == -1)
        // {
        // 	int location = GL.GetUniformLocation(ProgramId, uniformName);
        // 	_uLocationUMvp = location;
        // }

        var location = GL.GetUniformLocation(ProgramId, uniformName);

        GL.UniformMatrix4(location, 1, false, GetMatrix4X4Values(mat));
        // GL.UniformMatrix4(location, 1, false, GetMatrix4X4Values(mat));
        Uniforms[uniformName] = mat;
    }

    public void SetFloat(string uniformName, float fl)
    {
        var location = GL.GetUniformLocation(ProgramId, uniformName);
        GL.Uniform1(location, fl);
        Uniforms[uniformName] = fl;
    }

    public void SetVector2(string uniformName, Vector2 vec)
    {
        var location = GL.GetUniformLocation(ProgramId, uniformName);
        GL.Uniform2(location, vec.X, vec.Y);
        Uniforms[uniformName] = vec;
    }

    public void SetVector3(string uniformName, Vector3 vec)
    {
        var location = GL.GetUniformLocation(ProgramId, uniformName);
        GL.Uniform3(location, vec.X, vec.Y, vec.Z);
        Uniforms[uniformName] = vec;
    }

    public void SetVector3Array(string uniformName, float[] floats)
    {
        var location = GL.GetUniformLocation(ProgramId, uniformName);
        GL.Uniform3(location, floats.Length, floats);
        Uniforms[uniformName] = floats;
    }

    public void SetFloatArray(string uniformName, float[] floats)
    {
        var location = GL.GetUniformLocation(ProgramId, uniformName);
        GL.Uniform1(location, floats.Length, floats);
        Uniforms[uniformName] = floats;
    }

    public void SetVector4(string uniformName, Vector4 vec)
    {
        var location = GL.GetUniformLocation(ProgramId, uniformName);
        GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
        Uniforms[uniformName] = vec;
    }

    public void SetColor(string uniformName, Color col)
    {
        // if (_uLocationUColor == -1)
        // {
        var location = GL.GetUniformLocation(ProgramId, uniformName);
        // _uLocationUColor = location;
        // }

        GL.Uniform4(location, col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
        Uniforms[uniformName] = col;
    }

    public void SetColor(string uniformName, Vector4 vec)
    {
        // if (_uLocationUColor == -1)
        // {
        var location = GL.GetUniformLocation(ProgramId, uniformName);
        // _uLocationUColor = location;
        // }

        GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
        Uniforms[uniformName] = vec;
    }

    // uniform sampler2D textureObject;
    // just find "uniform" and 2 words after; then we know the variables and we can display it 
    // todo
    public ShaderUniform[] GetAllUniforms()
    {
        List<ShaderUniform> uniforms = new();

        Path = Path.Replace(@"\", "/");
        var filename = System.IO.Path.GetFileName(Path);

        Path = System.IO.Path.Combine("Assets", "Shaders", filename);

        if (File.Exists(Path) == false)
        {
            return new ShaderUniform[] { };
        }

        using (StreamReader sr = new(Path))
        {
            var shaderString = sr.ReadToEnd();
            var currentIndexInString = 0;
            var trimmedShaderString = shaderString;

            while (trimmedShaderString.Contains("uniform"))
            {
                var startIndex = trimmedShaderString.IndexOf("uniform");
                var endIndex = startIndex + trimmedShaderString.Substring(startIndex).IndexOf(";");

                var endIndexWithEqualsOperator = startIndex + trimmedShaderString.Substring(startIndex).IndexOf("=");

                if (endIndexWithEqualsOperator < endIndex) // if we have "=", trim it so it isnt in the name
                {
                    endIndex = endIndexWithEqualsOperator;
                }

                if (startIndex > endIndex)
                {
                    break;
                }

                ShaderUniform uniform = new();

                var uniformString = trimmedShaderString.Substring(startIndex, endIndex - startIndex).Split(' ');

                uniform.Name = uniformString[2];
                uniform.Type = GetUniformType(uniformString[1]);
                currentIndexInString = endIndex + (shaderString.Length - trimmedShaderString.Length);
                trimmedShaderString = shaderString.Substring(currentIndexInString);

                uniforms.Add(uniform);
            }
        }

        return uniforms.ToArray();
    }

    private Type GetUniformType(string typeName)
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

    private float[] GetMatrix4X4Values(Matrix4x4 m)
    {
        _getMatrix4X4ValuesArray[0] = m.M11;
        _getMatrix4X4ValuesArray[1] = m.M12;
        _getMatrix4X4ValuesArray[2] = m.M13;
        _getMatrix4X4ValuesArray[3] = m.M14;
        _getMatrix4X4ValuesArray[4] = m.M21;
        _getMatrix4X4ValuesArray[5] = m.M22;
        _getMatrix4X4ValuesArray[6] = m.M23;
        _getMatrix4X4ValuesArray[7] = m.M24;
        _getMatrix4X4ValuesArray[8] = m.M31;
        _getMatrix4X4ValuesArray[9] = m.M32;
        _getMatrix4X4ValuesArray[10] = m.M33;
        _getMatrix4X4ValuesArray[11] = m.M34;
        _getMatrix4X4ValuesArray[12] = m.M41;
        _getMatrix4X4ValuesArray[13] = m.M42;
        _getMatrix4X4ValuesArray[14] = m.M43;
        _getMatrix4X4ValuesArray[15] = m.M44;
        return _getMatrix4X4ValuesArray;
        /*
        return new[]
               {
                   m.M11, m.M12, m.M13, m.M14,
                   m.M21, m.M22, m.M23, m.M24,
                   m.M31, m.M32, m.M33, m.M34,
                   m.M41, m.M42, m.M43, m.M44
               };*/
    }

    public int GetAttribLocation(string attribName) => GL.GetAttribLocation(ProgramId, attribName);

    public static BufferType GetBufferTypeFromFileString(string shaderFile)
    {
        var typeString = shaderFile.Substring(shaderFile.IndexOf("["),
            shaderFile.IndexOf("]")); //File.ReadA;

        typeString = typeString.Substring(12);
        BufferType type;
        Enum.TryParse(typeString, out type);

        return type;
    }

    public static string GetVertexShaderFromFileString(string shaderFile) =>
        shaderFile.Substring(shaderFile.IndexOf("[VERTEX]") + 8,
            shaderFile.IndexOf("[FRAGMENT]") - shaderFile.IndexOf("[VERTEX]") - 8); //File.ReadA;

    public static string GetFragmentShaderFromFileString(string shaderFile) =>
        shaderFile.Substring(shaderFile.IndexOf("[FRAGMENT]") + 10); //File.ReadA;
}