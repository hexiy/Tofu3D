using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Tofu3D;

public class Window : GameWindow
{
    private bool _loaded;

    private float _monitorScale;

    public Window() : base(
        new GameWindowSettings(), // dont specify fps.... otherwise deltatime fucks up and update and render is called not 1:1
        new NativeWindowSettings
        {
            /*Size = new Vector2i(1, 1),*/
            APIVersion = new Version(4, 1), Flags = ContextFlags.ForwardCompatible,
            Profile = ContextProfile.Core /*NumberOfSamples = 8,*/,
            // WindowBorder = WindowBorder.Hidden,
            // WindowState = WindowState.Normal,
        })
    {
        // UpdateFrequency = 0;
        // RenderFrequency = 120;
        FrameLimiterEnabled = FrameLimiterEnabled;
        this.VSync = VSyncMode.Off;
        RenderFrequency = 120;
        FrameLimiterEnabled = FrameLimiterEnabled;

        LoadIcon();
        // LoadAndSetCursor();
        Title = WindowTitleText;
        // GL.Disable(EnableCap.Multisample);
    }

    public bool FrameLimiterEnabled
    {
        get
        {
            var v = PersistentData.GetBool("FrameLimiter", false);
            return v;
        }
        set
        {
            if (value)
            {
                RenderFrequency = 120;
                UpdateFrequency = 120;
            }
            else
            {
                RenderFrequency = 120;
                UpdateFrequency = 0;
            }

            PersistentData.Set("FrameLimiter", value);
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

    private unsafe void LoadAndSetCursor()
    {
        using (Image<Rgba32> image = Image.Load<Rgba32>(Path.Combine("Resources", "icon.png")))
        {
            // image.Mutate(ctx =>
            //     ctx.Flip(FlipMode
            //         .Vertical));

            byte[] pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            fixed (byte* pixelPtr = pixels)
            {
                
                OpenTK.Windowing.GraphicsLibraryFramework.Image glfwImage = new OpenTK.Windowing.GraphicsLibraryFramework.Image
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
        var image = Image.Load<Rgba32>(Path.Combine("Resources", "icon.png"));
        image.DangerousTryGetSinglePixelMemory(out var imageSpan);

        var imageBytes = MemoryMarshal.AsBytes(imageSpan.Span).ToArray();
        WindowIcon windowIcon = new(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes));

        Icon = windowIcon;
    }

    protected override unsafe void OnLoad()
    {
        GLFW.GetMonitorWorkarea((Monitor*)CurrentMonitor.Pointer, out var x, out var y, out var width, out var height);
        GLFW.GetMonitorContentScale((Monitor*)CurrentMonitor.Pointer, out _monitorScale, out _);
        Size = new Vector2i(width, height);

        Location = Vector2i.Zero;

        var secondaryMonitor = true;
        if (secondaryMonitor && GLFW.GetMonitors().Length > 1)
        {
            Location = Vector2i.Zero + new Vector2i(0, -height);
        }

        // WindowState = WindowState.Fullscreen;
        WindowState = WindowState.Maximized;

        Scene.AnySceneLoaded += () => { Title = WindowTitleText; };

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