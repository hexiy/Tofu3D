using System.Runtime.CompilerServices;
using System.Text;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

public class ImGuiController : IDisposable
{
    private static bool _khrDebugAvailable;

    private readonly List<char> _pressedChars = new List<char>();

    private readonly System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

    //private Texture _fontTexture;

    private int _fontTexture = -1;
    private bool _frameBegun;
    private int _indexBuffer;
    private int _indexBufferSize;

    private int _shader;
    private int _shaderTexture2DArrayLocation;
    private int _shaderTexture2DLocation;
    private int _shaderLayerLocation;
    private int _shaderIsArrayLocation;
    private int _shaderProjectionMatrixLocation;

    private int _vertexArray;
    private int _vertexBuffer;
    private int _vertexBufferSize;
    private int _windowHeight;

    private int _windowWidth;
    private Matrix4 mvp;
    private int _updatesThisSecond;
    private int _s;

    public int FontSizeFactorRelativeToDefault => (int)(Tofu.EditorSettingsAll.EditorSettingsGeneral.FontSize / 12f);
    private readonly Keys[] _keysArray;

    /// <summary>
    ///     Constructs a new ImGuiController.
    /// </summary>
    public ImGuiController()
    {
        _windowWidth = 500;
        _windowHeight = 500;

        int major = GL.GetInteger(GetPName.MajorVersion);
        int minor = GL.GetInteger(GetPName.MinorVersion);

        _khrDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        io.ConfigDockingAlwaysTabBar = true;
        io.ConfigWindowsResizeFromEdges = true;
        io.WantSaveIniSettings = false;

        // unsafe
        // {
        //     var filename = TofuPath.Combine(Folders.Data, "imgui.ini");
        //     byte[] filenameBytes = Encoding.UTF8.GetBytes(filename + "\0"); // needs null terminator
        //     fixed (byte* bytePtr = filenameBytes)
        //     {
        //         ImGui.GetIO().NativePtr->IniFilename = bytePtr;
        //     }
        //
        //     // ImGui.SaveIniSettingsToDisk(filename);
        // }

        // io.IniSavingRate = 5;

        LoadFont(Tofu.EditorSettingsAll.EditorSettingsGeneral.FontSize,
            TofuPath.Combine(Folders.EngineResourcesFonts, "inconsolata.ttf"));
        //io.Fonts.AddFontDefault();

        io.BackendFlags = ImGuiBackendFlags.None; // ImGuiBackendFlags.RendererHasVtxOffset;


        _keysArray = (Keys[])Enum.GetValues(typeof(Keys));

        CreateDeviceResources();
        SetKeyMappings();

        SetPerFrameImGuiData(1f / 120f);

        ImGui.NewFrame();
        _frameBegun = true;
    }

    public void LoadFont(int size, string path)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.Fonts.Clear();
        ImFontPtr fontPointer =
            io.Fonts.AddFontFromFileTTF(path,
                size * Screen.ScaleI);

        RecreateFontDeviceTexture();
        // ImGui.PushFont(fontPointer);
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

        int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
        int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

        _vertexArray = GL.GenVertexArray();
        Tofu.ShaderManager.BindVertexArray(_vertexArray);
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

        string vertexSource = @"#version 410 core
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
        string fragmentSource = @"#version 410 core
uniform sampler2DArray in_texture2DArray;
uniform sampler2D in_texture2D;
uniform int layerIndex;
uniform int isArray;
in vec4 color;
in vec2 texCoord;
out vec4 outputColor;
void main()
{
if(isArray == 0)
{
        outputColor = color * texture(in_texture2D, texCoord);
}
if(isArray == 1)
{
        outputColor = color * texture(in_texture2DArray, vec3(texCoord, layerIndex));
}
}";

        _shader = CreateProgram("ImGui", vertexSource, fragmentSource);
        TofuGL.CheckGlError("imgui setup -1");

        _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
        TofuGL.CheckGlError("imgui setup 0");

        _shaderTexture2DArrayLocation = GL.GetUniformLocation(_shader, "in_texture2DArray");
        TofuGL.CheckGlError("imgui setup 1");
        _shaderTexture2DLocation = GL.GetUniformLocation(_shader, "in_texture2D");
        _shaderLayerLocation = GL.GetUniformLocation(_shader, "layerIndex");
        _shaderIsArrayLocation = GL.GetUniformLocation(_shader, "isArray");

        // GL.Uniform1(_shaderTexture2DArrayLocation, 0); // set the uniform to unit 0
        // GL.Uniform1(_shaderTexture2DLocation, 1); // set the uniform to unit 1


        int stride = Unsafe.SizeOf<ImDrawVert>();
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);
        TofuGL.CheckGlError("imgui setup 2");

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);

        // Tofu.ShaderManager.BindVertexArray(prevVao);
        // GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);

        TofuGL.CheckGlError("End of ImGui setup");
    }

    /// <summary>
    ///     Recreates the device texture used to render text.
    /// </summary>
    public void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

        int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

        int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
        LabelObject(ObjectLabelIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte,
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
        if (!_frameBegun)
        {
            // If `Render` is called without starting the frame, invoke `ImGui.NewFrame()` first.
            ImGui.NewFrame();
        }

        // Render the ImGui frame:
        _frameBegun = false; // End the current frame
        ImGui.Render(); // Generate draw data

        // Render Draw Data using OpenGL or the configured renderer
        RenderImDrawData(ImGui.GetDrawData());
    }

    // private Stopwatch _fpsStopwatch = Stopwatch.StartNew();
    //
    // private void CalculateFramesPerSecond()
    // {
    //     // Count updates in the current second
    //     _updatesThisSecond++;
    //
    //     // Check if one second has elapsed
    //     if (_fpsStopwatch.Elapsed.TotalSeconds >= 1.0)
    //     {
    //         // Log/update FPS stats
    //         Debug.StatSetValue("imgui updates per second:", "imgui updates per second: " + _updatesThisSecond);
    //
    //         // Reset for the new second
    //         _fpsStopwatch.Restart();
    //         _updatesThisSecond = 0;
    //     }
    // }
    /// <summary>
    ///     Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(GameWindow wnd, float deltaSeconds)
    {
        // CalculateFramesPerSecond();


        // Ensure the frame begins (only once per update cycle)
        if (!_frameBegun)
        {
            ImGui.NewFrame();
            _frameBegun = true; // Mark the frame as active
        }

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput(wnd);
    }

    /// <summary>
    ///     Sets per-frame data based on the associated window.
    ///     This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(
            _windowWidth,
            _windowHeight);
        io.DisplayFramebufferScale = Vector2.One;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    private void UpdateImGuiInput(GameWindow wnd)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        MouseState? mouseState = wnd.MouseState;
        KeyboardState? keyboardState = wnd.KeyboardState;

        io.MouseDown[0] = mouseState[MouseButton.Left];
        io.MouseDown[1] = mouseState[MouseButton.Right];
        io.MouseDown[2] = mouseState[MouseButton.Middle];

        Vector2 screenPoint = new Vector2(mouseState.X, mouseState.Y);
        screenPoint *= Screen.ScaleI;

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

        foreach (char c in _pressedChars)
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
        ImGuiIOPtr io = ImGui.GetIO();

        io.MouseWheel = offset.Y;
        io.MouseWheelH = offset.X;
    }

    private static void SetKeyMappings()
    {
        ImGuiIOPtr io = ImGui.GetIO();
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
        int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
        int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
        int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
        bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
        bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
        int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
        int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
        int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
        int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
        int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
        int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
        bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
        bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
        int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
        GL.ActiveTexture(TextureUnit.Texture0);
        int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
        Span<int> prevScissorBox = stackalloc int[4];
        unsafe
        {
            fixed (int* iptr = &prevScissorBox[0])
            {
                GL.GetInteger(GetPName.ScissorBox, iptr);
            }
        }

        // Bind the element buffer (thru the VAO) so that we can resize it.
        Tofu.ShaderManager.BindVertexArray(_vertexArray);
        // Bind the vertex buffer so that we can resize it.
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);

        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            ImDrawListPtr cmdList = drawData.CmdListsRange[i];

            int vertexSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                int newSize = (int)Math.Max(_vertexBufferSize * 1.2f, vertexSize);

                GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _vertexBufferSize = newSize;

                // Debug.Log($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
            }

            int indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _indexBufferSize = newSize;

                //Debug.Log($"Resized dear imgui index buffer to new size {_indexBufferSize}");
            }
        }

        // Setup orthographic projection matrix into our constant buffer
        ImGuiIOPtr io = ImGui.GetIO();
        mvp = Matrix4.CreateOrthographicOffCenter(
            0.0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);

        GL.UseProgram(_shader);
        GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref mvp);
        GL.Uniform1(_shaderTexture2DArrayLocation, 0);
        TofuGL.CheckGlError("Projection");

        Tofu.ShaderManager.BindVertexArray(_vertexArray);
        TofuGL.CheckGlError("VAO");

        Vector2 scl = io.DisplayFramebufferScale;
        drawData.ScaleClipRects(scl);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.ScissorTest);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        // Render command lists
        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            ImDrawListPtr cmdList = drawData.CmdListsRange[n];

            GL.BufferData(BufferTarget.ArrayBuffer, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(),
                cmdList.VtxBuffer.Data, BufferUsageHint.StaticDraw);
            // GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero,
            //     cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmdList.VtxBuffer.Data);
            TofuGL.CheckGlError($"Data Vert {n}");

            GL.BufferData(BufferTarget.ElementArrayBuffer, cmdList.IdxBuffer.Size * sizeof(ushort),
                cmdList.IdxBuffer.Data, BufferUsageHint.StaticDraw);

            // GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmdList.IdxBuffer.Size * sizeof(ushort),
            //     cmdList.IdxBuffer.Data);
            TofuGL.CheckGlError($"Data Idx {n}");

            for (int cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
            {
                // continue;
                ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdI];
                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException();
                }


                //
                // int textureArrayID, layerIndex;
                // DecodeTextureArray(pcmd.TextureId, out textureArrayID, out layerIndex);
                //
                // GL.BindTexture(TextureTarget.Texture2DArray, textureArrayID); // Bind the texture array
                //
                // int layerUniformLocation = GL.GetUniformLocation(_shader, "layerIndex");
                // GL.Uniform1(layerUniformLocation, layerIndex);


                DecodeTextureId(pcmd.TextureId,
                    out bool isArrayTexture,
                    out int textureId,
                    out int layerIndex);


                GL.Uniform1(_shaderIsArrayLocation, isArrayTexture ? 1 : 0);

                if (isArrayTexture)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.Uniform1(_shaderTexture2DArrayLocation, 0); // set the uniform to unit 0

                    GL.BindTexture(TextureTarget.Texture2DArray, textureId);
                    GL.Uniform1(_shaderLayerLocation, layerIndex);
                }
                else
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.Uniform1(_shaderTexture2DLocation, 1); // set the uniform to unit 1

                    GL.BindTexture(TextureTarget.Texture2D, textureId);
                }


                TofuGL.CheckGlError("Texture");

                // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                System.Numerics.Vector4 clip = pcmd.ClipRect;
                GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                TofuGL.CheckGlError("Scissor");

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

                TofuGL.CheckGlError("Draw");
            }
        }

        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.ScissorTest);

        // Reset state
        GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
        GL.ActiveTexture((TextureUnit)prevActiveTexture);
        GL.UseProgram(prevProgram);
        Tofu.ShaderManager.BindVertexArray(prevVao);
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
        int n = GL.GetInteger(GetPName.NumExtensions);
        for (int i = 0; i < n; i++)
        {
            string? extension = GL.GetString(StringNameIndexed.Extensions, i);
            if (extension == name)
            {
                return true;
            }
        }

        return false;
    }

    public static int CreateProgram(string name, string vertexSource, string fragmentSoruce)
    {
        int program = GL.CreateProgram();
        LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {name}");

        int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
        int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);

        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string? info = GL.GetProgramInfoLog(program);
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
        int shader = GL.CreateShader(type);
        LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {name}");

        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string? info = GL.GetShaderInfoLog(shader);
            Debug.Log($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
        }

        return shader;
    }

    // public static IntPtr EncodeTextureArray(int textureArrayID, int layerIndex)
    // {
    //     // Combine textureArrayID and layerIndex into one IntPtr
    //     return (IntPtr)((textureArrayID & 0xFFFF) | ((layerIndex & 0xFFFF) << 16));
    // }
    public static IntPtr EncodeTextureId(int textureId)
    {
        return (IntPtr)textureId;
    }

    public static IntPtr EncodeTextureArrayId(RuntimeTexture runtimeTexture)
    {
        return EncodeTextureArrayId(runtimeTexture.AtlasGLTextureArrayId, runtimeTexture.IndexInAtlasTextureArray);
    }

    public static IntPtr EncodeTextureArrayId(int textureId, int layerIndex)
    {
        long encodedId = ((long)textureId) | (1L << 63); // set highest bit for "isArray"
        return (IntPtr)(encodedId | ((long)layerIndex << 32)); // put layer index in the upper bits
    }

    void DecodeTextureId(IntPtr textureId, out bool isArray, out int texture, out int layer)
    {
        long id = (long)textureId;

        // if high bit is set, its Texture2DArray, if not, its just Texture2D
        isArray = (id & (1L << 63)) != 0;

        // id
        texture = (int)(id & 0xFFFFFFFF); // lower 32 bits

        // layer in the texture array
        layer = isArray ? (int)((id >> 32) & 0x7FFFFFFF) : 0;
    }
    // public static void DecodeTextureArray(IntPtr textureId, out int textureArrayID, out int layerIndex)
    // {
    //     int id = textureId.ToInt32(); // Decode IntPtr back into textureArrayID and layerIndex
    //     textureArrayID = id & 0xFFFF;
    //     layerIndex = (id >> 16) & 0xFFFF;
    // }
}