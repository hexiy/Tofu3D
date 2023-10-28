using System.Runtime.CompilerServices;

namespace Dear_ImGui_Sample;

internal struct UniformFieldInfo
{
    public int Location;
    public string Name;
    public int Size;
    public ActiveUniformType Type;
}

internal class ImGuiShader
{
    private readonly (ShaderType Type, string Path)[] _files;
    public readonly string Name;
    private readonly Dictionary<string, int> _uniformToLocation = new();
    private bool _initialized;

    public ImGuiShader(string name, string vertexShader, string fragmentShader)
    {
        Name = name;
        _files = new[]
        {
            (ShaderType.VertexShader, vertexShader),
            (ShaderType.FragmentShader, fragmentShader)
        };
        Program = CreateProgram(name, _files);
    }

    public int Program { get; }

    public void UseShader()
    {
        Tofu.ShaderManager.ShaderInUse = Program;
        GL.UseProgram(Program);
    }

    public void Dispose()
    {
        if (_initialized)
        {
            GL.DeleteProgram(Program);
            _initialized = false;
        }
    }

    public UniformFieldInfo[] GetUniforms()
    {
        GL.GetProgram(Program, GetProgramParameterName.ActiveUniforms, out int unifromCount);

        var uniforms = new UniformFieldInfo[unifromCount];

        for (int i = 0; i < unifromCount; i++)
        {
            string name = GL.GetActiveUniform(Program, i, out int size, out ActiveUniformType type);

            UniformFieldInfo fieldInfo;
            fieldInfo.Location = GetUniformLocation(name);
            fieldInfo.Name = name;
            fieldInfo.Size = size;
            fieldInfo.Type = type;

            uniforms[i] = fieldInfo;
        }

        return uniforms;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetUniformLocation(string uniform)
    {
        if (_uniformToLocation.TryGetValue(uniform, out int location) == false)
        {
            location = GL.GetUniformLocation(Program, uniform);
            _uniformToLocation.Add(uniform, location);

            if (location == -1) Debug.Log($"The uniform '{uniform}' does not exist in the shader '{Name}'!");
        }

        return location;
    }

    private int CreateProgram(string name, params (ShaderType Type, string source)[] shaderPaths)
    {
        Util.CreateProgram(name, out int program);

        int[] shaders = new int[shaderPaths.Length];
        for (int i = 0; i < shaderPaths.Length; i++)
            shaders[i] = CompileShader(name, shaderPaths[i].Type, shaderPaths[i].source);

        foreach (int shader in shaders) GL.AttachShader(program, shader);

        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string info = GL.GetProgramInfoLog(program);
            Debug.Log($"GL.LinkProgram had info log [{name}]:\n{info}");
        }

        foreach (int shader in shaders)
        {
            GL.DetachShader(program, shader);
            GL.DeleteShader(shader);
        }

        _initialized = true;

        return program;
    }

    private int CompileShader(string name, ShaderType type, string source)
    {
        Util.CreateShader(type, name, out int shader);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string info = GL.GetShaderInfoLog(shader);
            Debug.Log($"GL.CompileShader for shader '{Name}' [{type}] had info log:\n{info}");
        }

        return shader;
    }
}