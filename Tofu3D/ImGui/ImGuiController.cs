﻿using System.Linq;
using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using OperatingSystem = Tofu3D.OperatingSystem;

public class ImGuiController : IDisposable
{
    private static bool _khrDebugAvailable;

    private readonly List<char> _pressedChars = new();

    private readonly System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

    //private Texture _fontTexture;

    private int _fontTexture;
    private bool _frameBegun;
    private int _indexBuffer;
    private int _indexBufferSize;

    private int _shader;
    private int _shaderFontTextureLocation;
    private int _shaderProjectionMatrixLocation;

    private int _vertexArray;
    private int _vertexBuffer;
    private int _vertexBufferSize;
    private int _windowHeight;

    private int _windowWidth;
    private Matrix4 mvp;
    private int _updatesThisSecond;
    private int _s;


    private readonly Keys[] _keysArray;

    /// <summary>
    ///     Constructs a new ImGuiController.
    /// </summary>
    public ImGuiController()
    {
        _windowWidth = 500;
        _windowHeight = 500;

        var major = GL.GetInteger(GetPName.MajorVersion);
        var minor = GL.GetInteger(GetPName.MinorVersion);

        _khrDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        io.ConfigDockingAlwaysTabBar = true;
        io.ConfigWindowsResizeFromEdges = true;
        io.WantSaveIniSettings = false;

        // io.IniSavingRate = 5;

        io.Fonts.AddFontFromFileTTF("inconsolata.ttf", 24);
        //io.Fonts.AddFontDefault();

        io.BackendFlags = ImGuiBackendFlags.None; // ImGuiBackendFlags.RendererHasVtxOffset;

        
        _keysArray = (Keys[])Enum.GetValues(typeof(Keys)); 
        
        CreateDeviceResources();
        SetKeyMappings();

        SetPerFrameImGuiData(1f / 120f);

        ImGui.NewFrame();
        _frameBegun = true;
    }

    /// <summary>
    ///     Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteBuffer(_indexBuffer);

        GL.DeleteTexture(_fontTexture);
        GL.DeleteProgram(_shader);
    }

    public void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    public void DestroyDeviceObjects()
    {
        Dispose();
    }

    public void CreateDeviceResources()
    {
        _vertexBufferSize = 180000;
        _indexBufferSize = 10000;

        var prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
        var prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

        _vertexArray = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArray);
        LabelObject(ObjectLabelIdentifier.VertexArray, _vertexArray, "ImGui");

        _vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        LabelObject(ObjectLabelIdentifier.Buffer, _vertexBuffer, "VBO: ImGui");
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        _indexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
        LabelObject(ObjectLabelIdentifier.Buffer, _indexBuffer, "EBO: ImGui");
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        RecreateFontDeviceTexture();

        var vertexSource = @"#version 410 core
uniform mat4 projection_matrix;
layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;
out vec4 color;
out vec2 texCoord;
void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
        var fragmentSource = @"#version 410 core
uniform sampler2D in_fontTexture;
in vec4 color;
in vec2 texCoord;
out vec4 outputColor;
void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

        _shader = CreateProgram("ImGui", vertexSource, fragmentSource);
        _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
        _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

        var stride = Unsafe.SizeOf<ImDrawVert>();
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(prevVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);

        CheckGlError("End of ImGui setup");
    }

    /// <summary>
    ///     Recreates the device texture used to render text.
    /// </summary>
    public void RecreateFontDeviceTexture()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height, out var bytesPerPixel);

        var mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

        var prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture0);
        var prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
        LabelObject(ObjectLabelIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte,
            pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        // Restore state
        GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
        GL.ActiveTexture((TextureUnit)prevActiveTexture);

        io.Fonts.SetTexID(_fontTexture);

        io.Fonts.ClearTexData();
    }

    /// <summary>
    ///     Renders the ImGui draw list data.
    /// </summary>
    public void Render()
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());
        }
    }

    /// <summary>
    ///     Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(GameWindow wnd, float deltaSeconds)
    {
        if (DateTime.Now.Second == _s)
        {
            _updatesThisSecond++;
        }
        else
        {
            Debug.StatSetValue("imgui per second:", "imgui per second:" + _updatesThisSecond);
            _s = DateTime.Now.Second;
            _updatesThisSecond = 0;
        }

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput(wnd);

    }

    public void X()
    {
        // if (_frameBegun)
        // {
        //     ImGui.Render();
        // }

        _frameBegun = true;
        ImGui.NewFrame(); 
    }
    /// <summary>
    ///     Sets per-frame data based on the associated window.
    ///     This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(
            _windowWidth,
            _windowHeight);
        io.DisplayFramebufferScale = Vector2.One;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    private void UpdateImGuiInput(GameWindow wnd)
    {
        var io = ImGui.GetIO();
        var mouseState = wnd.MouseState;
        var keyboardState = wnd.KeyboardState;

        io.MouseDown[0] = mouseState[MouseButton.Left];
        io.MouseDown[1] = mouseState[MouseButton.Right];
        io.MouseDown[2] = mouseState[MouseButton.Middle];

        Vector2 screenPoint = new(mouseState.X, mouseState.Y);
        if (OperatingSystem.IsMacOS)
        {
            screenPoint *= 2;
        }

        io.MousePos = new System.Numerics.Vector2(screenPoint.X, screenPoint.Y);

        // ReadOnlySpan<Keys> keysSpan = _keysArray;
        // foreach (Keys key in _keysArray)
        for (int i = 0; i < _keysArray.Length; i++)
        {
            if (_keysArray[i] == Keys.Unknown)
            {
                continue;
            }
            io.KeysDown[(int)_keysArray[i]] = keyboardState.IsKeyDown(_keysArray[i]);
        }

        foreach (var c in _pressedChars)
        {
            io.AddInputCharacter(c);
        }

        _pressedChars.Clear();

        io.KeyCtrl = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
        io.KeyAlt = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
        io.KeySuper = keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper);
        io.KeyShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
    }

    internal void PressChar(char keyChar)
    {
        _pressedChars.Add(keyChar);
    }

    internal void MouseScroll(Vector2 offset)
    {
        var io = ImGui.GetIO();

        io.MouseWheel = offset.Y;
        io.MouseWheelH = offset.X;
    }

    private static void SetKeyMappings()
    {
        var io = ImGui.GetIO();
        io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
        io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
        io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
        io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
        io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
        io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
        io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
        io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
        io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
    }

    private void RenderImDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0)
        {
            return;
        }

        // Get intial state.
        var prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
        var prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
        var prevProgram = GL.GetInteger(GetPName.CurrentProgram);
        var prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
        var prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
        var prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
        var prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
        var prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
        var prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
        var prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
        var prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
        var prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
        var prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
        var prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture0);
        var prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
        Span<int> prevScissorBox = stackalloc int[4];
        unsafe
        {
            fixed (int* iptr = &prevScissorBox[0])
            {
                GL.GetInteger(GetPName.ScissorBox, iptr);
            }
        }

        // Bind the element buffer (thru the VAO) so that we can resize it.
        GL.BindVertexArray(_vertexArray);
        // Bind the vertex buffer so that we can resize it.
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);

        for (var i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdListsRange[i];

            var vertexSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                var newSize = (int)Math.Max(_vertexBufferSize * 1.2f, vertexSize);

                GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _vertexBufferSize = newSize;

                Debug.Log($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
            }

            var indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                var newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _indexBufferSize = newSize;

                //Debug.Log($"Resized dear imgui index buffer to new size {_indexBufferSize}");
            }
        }

        // Setup orthographic projection matrix into our constant buffer
        var io = ImGui.GetIO();
        mvp = Matrix4.CreateOrthographicOffCenter(
            0.0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);

        GL.UseProgram(_shader);
        GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref mvp);
        GL.Uniform1(_shaderFontTextureLocation, 0);
        CheckGlError("Projection");

        GL.BindVertexArray(_vertexArray);
        CheckGlError("VAO");

        Vector2 scl = io.DisplayFramebufferScale;
        drawData.ScaleClipRects(scl);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.ScissorTest);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        // Render command lists
        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdListsRange[n];

            GL.BufferData(BufferTarget.ArrayBuffer, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(),
                cmdList.VtxBuffer.Data, BufferUsageHint.StaticDraw);
            // GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero,
            //     cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmdList.VtxBuffer.Data);
            CheckGlError($"Data Vert {n}");

            GL.BufferData(BufferTarget.ElementArrayBuffer, cmdList.IdxBuffer.Size * sizeof(ushort),
                cmdList.IdxBuffer.Data, BufferUsageHint.StaticDraw);

            // GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmdList.IdxBuffer.Size * sizeof(ushort),
            //     cmdList.IdxBuffer.Data);
            CheckGlError($"Data Idx {n}");

            for (var cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
            {
                // continue;
                var pcmd = cmdList.CmdBuffer[cmdI];
                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException();
                }

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                CheckGlError("Texture");

                // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                var clip = pcmd.ClipRect;
                GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                CheckGlError("Scissor");

                if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                {
                    GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount,
                        DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)),
                        unchecked((int)pcmd.VtxOffset));
                }
                else
                {
                    GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort,
                        (int)pcmd.IdxOffset * sizeof(ushort));
                }

                CheckGlError("Draw");
            }
        }

        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.ScissorTest);

        // Reset state
        GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
        GL.ActiveTexture((TextureUnit)prevActiveTexture);
        GL.UseProgram(prevProgram);
        GL.BindVertexArray(prevVao);
        GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
        GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
        GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
        GL.BlendFuncSeparate(
            (BlendingFactorSrc)prevBlendFuncSrcRgb,
            (BlendingFactorDest)prevBlendFuncDstRgb,
            (BlendingFactorSrc)prevBlendFuncSrcAlpha,
            (BlendingFactorDest)prevBlendFuncDstAlpha);
        if (prevBlendEnabled)
        {
            GL.Enable(EnableCap.Blend);
        }
        else
        {
            GL.Disable(EnableCap.Blend);
        }

        if (prevDepthTestEnabled)
        {
            GL.Enable(EnableCap.DepthTest);
        }
        else
        {
            GL.Disable(EnableCap.DepthTest);
        }

        if (prevCullFaceEnabled)
        {
            GL.Enable(EnableCap.CullFace);
        }
        else
        {
            GL.Disable(EnableCap.CullFace);
        }

        if (prevScissorTestEnabled)
        {
            GL.Enable(EnableCap.ScissorTest);
        }
        else
        {
            GL.Disable(EnableCap.ScissorTest);
        }
    }

    public static void LabelObject(ObjectLabelIdentifier objLabelIdent, int glObject, string name)
    {
        if (_khrDebugAvailable)
        {
            GL.ObjectLabel(objLabelIdent, glObject, name.Length, name);
        }
    }

    private static bool IsExtensionSupported(string name)
    {
        var n = GL.GetInteger(GetPName.NumExtensions);
        for (var i = 0; i < n; i++)
        {
            var extension = GL.GetString(StringNameIndexed.Extensions, i);
            if (extension == name)
            {
                return true;
            }
        }

        return false;
    }

    public static int CreateProgram(string name, string vertexSource, string fragmentSoruce)
    {
        var program = GL.CreateProgram();
        LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {name}");

        var vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
        var fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);

        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var success);
        if (success == 0)
        {
            var info = GL.GetProgramInfoLog(program);
            Debug.Log($"GL.LinkProgram had info log [{name}]:\n{info}");
        }

        GL.DetachShader(program, vertex);
        GL.DetachShader(program, fragment);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return program;
    }

    private static int CompileShader(string name, ShaderType type, string source)
    {
        var shader = GL.CreateShader(type);
        LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {name}");

        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
        if (success == 0)
        {
            var info = GL.GetShaderInfoLog(shader);
            Debug.Log($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
        }

        return shader;
    }

    public static bool CheckGlError(string title)
    {
        var hadError = false;
        ErrorCode error;
        var i = 1;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            Debug.LogError($"{title} ({i++}): {error}");
            hadError = true;
        }

        return hadError;
    }
}