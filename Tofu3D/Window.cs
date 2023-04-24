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
	public ImGuiController ImGuiController;

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
	}

	private void LoadIcon()
	{
		Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(@"Resources\icon.png");
		image.DangerousTryGetSinglePixelMemory(out var imageSpan);

		byte[] imageBytes = MemoryMarshal.AsBytes(imageSpan.Span).ToArray();
		WindowIcon windowIcon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes));

		this.Icon = windowIcon;
	}

	public string WindowTitleText
	{
		get { return $"Tofu3D | {GL.GetString(StringName.Version)}"; }
	}
	bool _loaded = false;

	protected override unsafe void OnLoad()
	{
		GLFW.GetMonitorWorkarea((Monitor*) this.CurrentMonitor.Pointer, out int x, out int y, out int width, out int height);

		Size = new Vector2i(width, height);
		Location = Vector2i.Zero;
		ImGuiController = new ImGuiController(width, height);


		// WindowState = WindowState.Fullscreen;
		WindowState = WindowState.Maximized;

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

		// Update the opengl viewport
		//GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

		// Tell ImGui of the new size
		ImGuiController?.WindowResized(ClientSize.X, ClientSize.Y);
	}

	protected override void OnUpdateFrame(FrameEventArgs e)
	{
		if (_loaded == false) return;

		base.OnUpdateFrame(e);
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		if (_loaded == false) return;

		Time.EditorDeltaTime = (float) (e.Time);


		Debug.StartGraphTimer("Window Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1 / 60f), -1);

		Debug.StartGraphTimer("Scene Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1f / 60f));

		RenderPassSystem.RenderAllPasses();

		Debug.EndGraphTimer("Scene Render");


		Debug.StartGraphTimer("ImGui", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromMilliseconds(2));

		bool renderImGui = true;
		if (renderImGui)
		{
			ImGuiController.Update(this, (float) e.Time);
			GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

			ImGuiController.WindowResized(ClientSize.X, ClientSize.Y);

			Tofu.I.Editor.Draw();

			ImGuiController.Render();
		}

		Debug.EndGraphTimer("ImGui");

		SwapBuffers();

		base.OnRenderFrame(e);

		Debug.EndGraphTimer("Window Render");

		Debug.ResetTimers();
		Debug.ClearAdditiveStats();
	}

	protected override void OnTextInput(TextInputEventArgs e)
	{
		base.OnTextInput(e);

		ImGuiController.PressChar((char) e.Unicode);
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);

		ImGuiController.MouseScroll(new Vector2(e.OffsetX, e.OffsetY));
	}
}