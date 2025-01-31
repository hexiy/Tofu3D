using System.IO;
using Newtonsoft.Json;

namespace Tofu3D;

[Serializable]
public class
    Shader : IDisposable
{
    private const string UniformName_TextureAlbedo = "u_albedoTexture";
    private const string UniformName_TextureAlphaMask = "u_alphaMaskTexture";
    private const string UniformName_TextureNormal = "u_normalTexture";
    private const string UniformName_TextureAo = "u_ambientOcclusionTexture";
    private const string UniformName_ShadowMap = "u_shadowmapTexture";
    private const string UniformName_EnvironmentCubemap = "u_environmentCubemap";
    private const string UniformName_TextureObject = "u_textureObject";
    private const string UniformName_BloomThresholdTexture = "u_bloomThresholdTexture";
    private const string UniformName_HorizontalBlurTexture = "u_horizontalBlurTexture";
    private const string UniformName_VerticalBlurTexture = "u_verticalBlurTexture";
    private const string UniformName_TextureRoughness = "u_roughnessTexture";
    private const string UniformName_TextureMetallic = "u_metallicTexture";
    private const string UniformName_TextureEmissive = "u_emissiveTexture";
    private const string UniformName_ScreenColor = "u_screenColor";
    private const string UniformName_DepthMap = "u_depthMap";
    public const string UniformName_AtlasTexture = "textureArray";

    private float[] _getMatrix4X4ValuesArray =
    {
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0
    };

    [JsonIgnore]
    [XmlIgnore]
    public TextureUnit? AtlasArrayTextureUnit = null;

    [JsonIgnore]
    [XmlIgnore]
    public TextureUnit? ShadowMapTextureUnit = null;

    [JsonIgnore]
    [XmlIgnore]
    public TextureUnit? EnvironmentTextureUnit = null;

    [JsonIgnore]
    [XmlIgnore]
    public TextureUnit? ScreenColorUnit = null;

    [JsonIgnore]
    [XmlIgnore]
    public TextureUnit? DepthMapUnit = null;

    public BufferType BufferType;

    public string Path;

    public bool AtlasUniformIsSet = false;
    // [JsonIgnore]
    // [XmlIgnore]
    // public Dictionary<string, object> Uniforms = new()
    // {
    // };

    [JsonIgnore]
    [XmlIgnore]
    public Dictionary<string, int> UniformLocations = new Dictionary<string, int>
    {
    };

    public Shader()
    {
    }

    public Shader(string filePath)
    {
        Path = filePath;
    }

    [JsonIgnore]
    [XmlIgnore]
    public bool IsLoaded { get; private set; }

    [JsonIgnore]
    [XmlIgnore]
    public int ProgramId { get; set; }

    public void Dispose()
    {
        // GL.DeleteProgram(ProgramId); // dont really do this since multiple materials can be using the shader
    }

    // make Uniforms List<ShaderUniform> and get index from that
    public int GetUniformLocation(string uniformName) => GL.GetUniformLocation(ProgramId, uniformName);

    public void Load( /*Asset_Material material*/)
    {
        AssetPathExtensions.ValidateAssetPath(ref Path);

        if (AssetPathExtensions.Exists(Path) == false)
        {
            string newPath = TofuPath.Combine(Folders.Assets, Path);
            if (AssetPathExtensions.Exists(newPath))
            {
                Path = newPath;
            }
        }

        if (Path.Contains(
                ".mat")) // IF ITS mat  not .glsl, just assign SpriteRenderer so we can fix it without crashing
        {
            Path = TofuPath.Combine(Folders.ShadersInAssets, "SpriteRenderer.glsl");
        }

        if (AssetPathExtensions.Exists(Path) == false)
        {
            Debug.Log($"Couldn't find shader:{Path}");
            // throw new FileNotFoundException("Couldn't find shader");
            return;
        }

        // GetAllUniforms();
        string shaderFile = File.ReadAllText(Path);

        // set defines here 

        string vertexCode = GetVertexShaderFromFileString(shaderFile);
        string fragmentCode = GetFragmentShaderFromFileString(shaderFile);

        // ProcessShader(material, ref vertexCode, ref fragmentCode);

        BufferType = GetBufferTypeFromFileString(shaderFile);


        int vs, fs;

        vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vs, vertexCode);
        GL.CompileShader(vs);

        string? error = "";
        GL.GetShaderInfoLog(vs, out error);
        if (error.Length > 0)
        {
            Debug.LogError("ERROR COMPILING VERTEX SHADER " + error);
        }

        fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fs, fragmentCode);
        GL.CompileShader(fs);

        error = "";
        GL.GetShaderInfoLog(fs, out error);
        if (error.Length > 0)
        {
            Debug.LogError("ERROR COMPILING VERTEX SHADER " + error);
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


        Tofu.ShaderManager.UseShader(this, forceUse: true);

        List<string> textureUniformsNames = new List<string>
        {
            UniformName_TextureAlbedo,
            UniformName_TextureAlphaMask,
            UniformName_TextureNormal,
            UniformName_TextureAo,
            UniformName_ShadowMap,
            UniformName_EnvironmentCubemap,
            UniformName_TextureObject,
            UniformName_BloomThresholdTexture,
            UniformName_HorizontalBlurTexture,
            UniformName_VerticalBlurTexture,
            UniformName_TextureRoughness,
            UniformName_TextureMetallic,
            UniformName_TextureEmissive,
            UniformName_ScreenColor,
            UniformName_DepthMap
        };
        // AlbedoTextureLocation = GetUniformLocation("textureAlbedo");
        // NormalTextureLocation = GetUniformLocation("textureNormal");
        // AmbientOcclusionTextureLocation = GetUniformLocation("textureAo");
        // ShadowMapTextureLocation = GetUniformLocation("shadowMap");
        //
        // int bloomTextureLocation = GetUniformLocation("textureObject");
        // int bloomThresholdLocation = GetUniformLocation("bloomThresholdTexture");

        // GL.Uniform1 to bind the texture to the texture unit-Texture0, Texture1 etc
        int textureUnitsCount = 0;
        for (int index = 0; index < textureUniformsNames.Count; index++)
        {
            string textureUniformName = textureUniformsNames[index];
            int location = GetUniformLocation(textureUniformName);
            if (location == -1)
            {
                continue;
            }

            TextureUnit textureUnit = TextureUnit.Texture0 + textureUnitsCount;

            switch (textureUniformName)
            {
                // case UniformName_TextureAlbedo:
                // AlbedoTextureIndexUnit = textureUnit;
                // break;
                // case UniformName_TextureAlphaMask:
                // AlphaMaskTextureIndexUnit = textureUnit;
                // break;
                // case UniformName_TextureNormal:
                // NormalTextureIndexUnit = textureUnit;
                // break;
                // case UniformName_TextureAo:
                // AmbientOcclusionTextureUnit = textureUnit;
                // break;
                case UniformName_AtlasTexture:
                    AtlasArrayTextureUnit = textureUnit;
                    break;
                case UniformName_ShadowMap:
                    ShadowMapTextureUnit = textureUnit;
                    break;
                case UniformName_EnvironmentCubemap:
                    EnvironmentTextureUnit = textureUnit;
                    break;
                case UniformName_TextureObject:
                    break;
                case UniformName_BloomThresholdTexture:
                    break;
                case UniformName_HorizontalBlurTexture:
                    break;
                case UniformName_VerticalBlurTexture:
                    break;
                // case UniformName_TextureRoughness:
                // RoughnessTextureUnit = textureUnit;
                // break;
                // case UniformName_TextureMetallic:
                // MetallicTextureUnit = textureUnit;
                // break;
                // case UniformName_TextureEmissive:
                // EmissiveTextureUnit = textureUnit;
                // break;
                case UniformName_ScreenColor:
                    ScreenColorUnit = textureUnit;
                    break;
                case UniformName_DepthMap:
                    DepthMapUnit = textureUnit;
                    break;
            }

            GL.Uniform1(location, textureUnitsCount);
            textureUnitsCount++;
        }
        // if (AlbedoTextureLocation != -1)
        // {
        //     GL.Uniform1(AlbedoTextureLocation, indx);
        //     indx++;
        // }
        //
        // if (NormalTextureLocation != -1)
        // {
        //     GL.Uniform1(NormalTextureLocation, indx);
        //     indx++;
        // }
        //
        // if (AmbientOcclusionTextureLocation != -1)
        // {
        //     GL.Uniform1(AmbientOcclusionTextureLocation, indx);
        //     indx++;
        // }
        //
        // if (ShadowMapTextureLocation != -1)
        // {
        //     GL.Uniform1(ShadowMapTextureLocation, indx);
        //     indx++;
        // }
        //
        // if (bloomTextureLocation != -1)
        // {
        //     GL.Uniform1(bloomTextureLocation, indx);
        //     indx++;
        // }
        //
        // if (bloomThresholdLocation != -1)
        // {
        //     GL.Uniform1(bloomThresholdLocation, indx);
        //     indx++;
        // }

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

    // private void ProcessShader(Asset_Material material, ref string vertexShader, ref string fragmentShader)
    // {
    //     // return;
    //     string uvOffsetIsInstancedDefine =
    //         $"\n#define UV_OFFSET_IS_INSTANCED {(material.UVOffsetIsInstanced ? "1" : "0")}";
    //
    //     int newLineIndexInVertex = vertexShader.IndexOf("#version 410 core") + "#version 410 core".Length;
    //     vertexShader = vertexShader.Insert(newLineIndexInVertex, uvOffsetIsInstancedDefine);
    //
    //     int newLineIndexInFragment = fragmentShader.IndexOf("#version 410 core") + "#version 410 core".Length;
    //     fragmentShader = fragmentShader.Insert(newLineIndexInFragment, uvOffsetIsInstancedDefine);
    // }

    public void SetMatrix4X4(string uniformName, Matrix4x4 mat)
    {
        // if (_uLocationUMvp == -1)
        // {
        // 	int location = GL.GetUniformLocation(ProgramId, uniformName);
        // 	_uLocationUMvp = location;
        // }

        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.UniformMatrix4(location, 1, false, GetMatrix4X4Values(mat));
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.UniformMatrix4(location, 1, false, GetMatrix4X4Values(mat));
        }

        // GL.UniformMatrix4(location, 1, false, GetMatrix4X4Values(mat));
        // Uniforms[uniformName] = mat;
    }

    public void SetFloat(string uniformName, float fl)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform1(location, fl);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform1(location, fl);
        }

        // Uniforms[uniformName] = fl;
    }

    public void SetInt(string uniformName, int num)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform1(location, num);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform1(location, num);
        }

        //Uniforms[uniformName] = num;
    }

    public void SetVector2(string uniformName, Vector2 vec)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform2(location, vec.X, vec.Y);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform2(location, vec.X, vec.Y);
        }
        // Uniforms[uniformName] = vec;
    }

    public void SetVector3(string uniformName, Vector3 vec)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform3(location, vec.X, vec.Y, vec.Z);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform3(location, vec.X, vec.Y, vec.Z);
        }
        // Uniforms[uniformName] = vec;
    }

    public void SetVector3Array(string uniformName, float[] floats)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform3(location, floats.Length, floats);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform3(location, floats.Length, floats);
        }
        // Uniforms[uniformName] = floats;
    }

    public void SetFloatArray(string uniformName, float[] floats)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform1(location, floats.Length, floats);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform1(location, floats.Length, floats);
        }
        // Uniforms[uniformName] = floats;
    }

    public void SetVector4(string uniformName, Vector4 vec)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
        }
        // Uniforms[uniformName] = vec;
    }

    public void SetColor(string uniformName, Color col)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform4(location, col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform4(location, col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f);
        }
        // Uniforms[uniformName] = col;
    }

    public void SetColor(string uniformName, Vector4 vec)
    {
        if (UniformLocations.TryGetValue(uniformName, out int location))
        {
            GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
        }
        else
        {
            location = GL.GetUniformLocation(ProgramId, uniformName);
            UniformLocations[uniformName] = location;

            GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
        }
        // Uniforms[uniformName] = vec;
    }

    // uniform sampler2D textureObject;
    // just find "uniform" and 2 words after; then we know the variables and we can display it 
    // todo
    public ShaderUniform[] GetAllUniforms()
    {
        List<ShaderUniform> uniforms = new List<ShaderUniform>();

        Path = Path.Replace(@"\", "/");
        string filename = System.IO.Path.GetFileName(Path);

        Path = TofuPath.Combine("Assets", "Shaders", filename);

        if (File.Exists(Path) == false)
        {
            return new ShaderUniform[] { };
        }

        using (StreamReader sr = new StreamReader(Path))
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

                ShaderUniform uniform = new ShaderUniform();

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
        string typeString = shaderFile.Substring(shaderFile.IndexOf("["),
            shaderFile.IndexOf("]") - 2); //File.ReadA;

        typeString = typeString.Substring(13);
        BufferType type;
        Enum.TryParse(typeString, out type);

        return type;
    }

    public static string GetVertexShaderFromFileString(string shaderFile)
    {
        int vertexTagIndex = shaderFile.IndexOf("//[VERTEX]");
        int fragmentTagIndex = shaderFile.IndexOf("//[FRAGMENT]");
        int startIndex = vertexTagIndex + "//[VERTEX]".Length;
        int length = fragmentTagIndex - vertexTagIndex - "//[VERTEX]".Length;
        if (length < 0)
        {
            return string.Empty;
        }

        return shaderFile.Substring(startIndex, length);
    }

    public static string GetFragmentShaderFromFileString(string shaderFile)
    {
        return shaderFile.Substring(shaderFile.LastIndexOf("//[FRAGMENT]") + "//[FRAGMENT]".Length);
    }
}