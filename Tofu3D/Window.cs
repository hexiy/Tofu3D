using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using Tofu3D.Rendering;

namespace Tofu3D;

public class Window : GameWindow
{
	bool _loaded = false;

	public Window() : base(new GameWindowSettings() { }, // dont specify fps.... otherwise deltatime fucks up and update and render is called not 1:1
	                       new NativeWindowSettings
	                       {
		                       /*Size = new Vector2i(1, 1),*/ APIVersion = new Version(4, 1), Flags = ContextFlags.ForwardCompatible, Profile = ContextProfile.Core, /*NumberOfSamples = 8,*/
	                       })
	{
		VSync = VSyncMode.On;
		// this.UpdateFrequency = 60;
		// this.RenderFrequency = 0;
		LoadIcon();
		Title = WindowTitleText;
		GLFW.WindowHint(WindowHintBool.Decorated, false);
		GL.Disable(EnableCap.Multisample);
	}

	private void LoadIcon()
	{
		Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(Path.Combine("Resources", "icon.png"));
		image.DangerousTryGetSinglePixelMemory(out var imageSpan);

		byte[] imageBytes = MemoryMarshal.AsBytes(imageSpan.Span).ToArray();
		WindowIcon windowIcon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes));

		this.Icon = windowIcon;
	}

	public Vector2 WindowSize
	{
		get { return new Vector2(Size.X, Size.Y); }
	}

	private float _monitorScale;
	public float MonitorScale => _monitorScale;
	public string WindowTitleText
	{
		get { return $"Tofu3D | {GL.GetString(StringName.Version)}"; }
	}

	protected override unsafe void OnLoad()
	{
		GLFW.GetMonitorWorkarea((Monitor*) this.CurrentMonitor.Pointer, out int x, out int y, out int width, out int height);
		GLFW.GetMonitorContentScale((Monitor*) this.CurrentMonitor.Pointer, out _monitorScale, out _);
		Size = new Vector2i(width, height);

		Location = Vector2i.Zero;

		bool secondaryMonitor = true;
		if (secondaryMonitor && GLFW.GetMonitors().Length>1)
		{
			Location = Vector2i.Zero + new Vector2i(0, -height);
		}

		// WindowState = WindowState.Fullscreen;
		WindowState = WindowState.Maximized;

		Scene.AnySceneLoaded += () => { Title = $"{WindowTitleText} | {Tofu.I.SceneManager.CurrentScene?.SceneName}"; };

		this.Focus();

		base.OnLoad();
		_loaded = true;
	}

	protected override void OnUnload()
	{
		base.OnUnload();
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		if (_loaded == false) return;
		base.OnResize(e);
		
		Tofu.I.ImGuiController?.WindowResized(ClientSize.X, ClientSize.Y);
	}

	protected override void OnUpdateFrame(FrameEventArgs e)
	{
		if (_loaded == false) return;
		
		base.OnUpdateFrame(e);
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		if (_loaded == false) return;
		base.OnRenderFrame(e);
	}

	protected override void OnTextInput(TextInputEventArgs e)
	{
		base.OnTextInput(e);
		Tofu.I.ImGuiController.PressChar((char) e.Unicode);
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);
		Tofu.I.ImGuiController.MouseScroll(new Vector2(e.OffsetX, e.OffsetY));
	}
}