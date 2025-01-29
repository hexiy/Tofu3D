using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace Tofu3D;

public class Window : GameWindow
{
    private bool _loaded;

    private float _monitorScale;


    private const double MaxAllowedFrameRate_NoLimiter = 1000.0; // 1000 FPS max
    private const double MaxAllowedFrameRate_Limiter = 120; // 1000 FPS max

    private readonly double
        _minFrameTimeLimit_NoLimiter =
            1.0 / MaxAllowedFrameRate_NoLimiter; // Time per frame in seconds (1ms for 1000 FPS limit)

    private readonly double
        _minFrameTimeLimit_Limiter =
            1.0 / MaxAllowedFrameRate_Limiter; // Time per frame in seconds (1ms for 1000 FPS limit)

    private double _lastFrameTime = 0.0;
    private bool _framLimiterEnabled = false;

    public bool FrameLimiterEnabled
    {
        get
        {
            _framLimiterEnabled = PersistentData.GetBool("FrameLimiter", false);
            return _framLimiterEnabled;
        }
        set
        {
            if (value)
            {
                RenderFrequency = 1;
                UpdateFrequency = MaxAllowedFrameRate_NoLimiter;
            }
            else
            {
                RenderFrequency = 1;
                UpdateFrequency = 0; // unlimited
            }

            PersistentData.Set("FrameLimiter", value);
            _framLimiterEnabled = value;
        }
    }

    public Vector2 WindowSize => new(Size.X, Size.Y);

    public Vector2 WindowPosition { get; private set; }
    public float MonitorScale => _monitorScale;

    // public string WindowTitleText =>
    // $"Tofu3D | {GL.GetString(StringName.Version)} | {Tofu.SceneManager.CurrentScene?.SceneName} | {WindowSize}";
    StringBuilder titleStringBuilder = new StringBuilder();

    public string WindowTitleText
    {
        get
        {
            titleStringBuilder.Clear();
            titleStringBuilder.Append("Tofu3D | ");
            titleStringBuilder.Append(GL.GetString(StringName.Version));
            titleStringBuilder.Append(" | ");
            titleStringBuilder.Append(Tofu.SceneManager.CurrentScene?.SceneName ?? "No Scene");
            titleStringBuilder.Append(" | ");
            titleStringBuilder.Append(WindowSize);

            return titleStringBuilder.ToString();
        }
    }

    public Window() : base(
        new GameWindowSettings(),
        new NativeWindowSettings
        {
            APIVersion = new Version(4, 1), Flags = ContextFlags.ForwardCompatible,
            Profile = ContextProfile.Core /*NumberOfSamples = 8,*/,
            // WindowBorder = WindowBorder.Hidden,
            // WindowState = WindowState.Normal,
        })
    {
        this.VSync = VSyncMode.Off;
        FrameLimiterEnabled = FrameLimiterEnabled;

        LoadIcon();
        // LoadAndSetCursor();
        Title = WindowTitleText;
        GL.Disable(EnableCap.Multisample);
    }

    private unsafe void LoadAndSetCursor()
    {
        using (Image<Rgba32> image = Image.Load<Rgba32>(TofuPath.Combine("Resources", "icon.png")))
        {
            // image.Mutate(ctx =>
            //     ctx.Flip(FlipMode
            //         .Vertical));

            byte[] pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            fixed (byte* pixelPtr = pixels)
            {
                OpenTK.Windowing.GraphicsLibraryFramework.Image glfwImage =
                    new OpenTK.Windowing.GraphicsLibraryFramework.Image
                    {
                        Width = image.Width,
                        Height = image.Height,
                        Pixels = pixelPtr
                    };

                Cursor* cursor = GLFW.CreateCursor(ref glfwImage, 0, 0);
                GLFW.SetCursor(this.WindowPtr, cursor);
            }
        }
    }

    private void LoadIcon()
    {
        Image<Rgba32>? image = Image.Load<Rgba32>(TofuPath.Combine("Resources", "icon.png"));
        image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> imageSpan);

        byte[] imageBytes = MemoryMarshal.AsBytes(imageSpan.Span).ToArray();
        WindowIcon windowIcon = new(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes));

        Icon = windowIcon;
    }

    protected override unsafe void OnLoad()
    {
        GLFW.GetMonitorWorkarea((Monitor*)CurrentMonitor.Pointer, out int x, out int y, out int width, out int height);
        GLFW.GetMonitorContentScale((Monitor*)CurrentMonitor.Pointer, out _monitorScale, out _);
        Size = new Vector2i(width, height);

        Location = Vector2i.Zero;

        bool secondaryMonitor = true;
        if (secondaryMonitor && GLFW.GetMonitors().Length > 1)
        {
            Location = Vector2i.Zero + new Vector2i(0, -height);
        }

        // WindowState = WindowState.Fullscreen;
        WindowState = WindowState.Maximized;

        Scene.SceneLoaded += () => { Title = WindowTitleText; };

        Focus();

        base.OnLoad();
        _loaded = true;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        // if (_loaded == false) return;
        base.OnResize(e);

        Tofu.ImGuiController?.WindowResized(ClientSize.X, ClientSize.Y);
    }

    protected override void OnMove(WindowPositionEventArgs e)
    {
        WindowPosition = new Vector2(e.Position.X, e.Position.Y);
        base.OnMove(e);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        if (_loaded == false)
        {
            return;
        }

        // Title = WindowTitleText;
        base.OnUpdateFrame(e);
    }


    public void ManageFrameLimiter()
    {
        double currentTime = GLFW.GetTime(); // GLFW time in seconds
        // double deltaTime = currentTime - _lastFrameTime;

        // if ((_framLimiterEnabled && deltaTime < _minFrameTimeLimit_Limiter) ||
        //     (_framLimiterEnabled == false && deltaTime < _minFrameTimeLimit_NoLimiter))
        // {
        //     double waitTime = (_framLimiterEnabled
        //         ? _minFrameTimeLimit_Limiter
        //         : _minFrameTimeLimit_NoLimiter) - deltaTime;
        //     Thread.Sleep((int)(waitTime * 1000)); // Convert to milliseconds for Thread.Sleep
        // }

        double elapsedTime = currentTime - _lastFrameTime;
        while (elapsedTime < (_framLimiterEnabled
                   ? _minFrameTimeLimit_Limiter
                   : _minFrameTimeLimit_NoLimiter))
        {
            currentTime = GLFW.GetTime();
            elapsedTime = currentTime - _lastFrameTime;
        }

        _lastFrameTime = currentTime;
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        if (_loaded == false)
        {
            return;
        }

        base.OnRenderFrame(e);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        Tofu.ImGuiController.PressChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        Tofu.ImGuiController.MouseScroll(new Vector2(e.OffsetX, e.OffsetY));
    }
}