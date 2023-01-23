using Engine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Tofu3D.Rendering;

namespace Tofu3D;

public class Window : GameWindow
{
	public ImGuiController ImGuiController;

	public Window() : base(new GameWindowSettings() {UpdateFrequency = 60, RenderFrequency = 60},
	                       new NativeWindowSettings
	                       {Size = new Vector2i(2560, 1600), APIVersion = new Version(4, 1), Flags = ContextFlags.ForwardCompatible, Profile = ContextProfile.Core, NumberOfSamples = 8})
	{
		I = this;
		WindowState = WindowState.Maximized;
		//WindowState = WindowState.Fullscreen;
	}

	public static Window I { get; private set; }

	public string WindowTitleText
	{
		get { return $"Tofu3D | {GL.GetString(StringName.Version)}"; }
	}

	protected override void OnLoad()
	{
		Title = WindowTitleText;

		ImGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);

		Editor.I.Init();
		Scene.I.Start();

		RenderPassSystem.Initialize();
		MousePickingSystem.Initialize();
	}

	protected override void OnUnload()
	{
		Scene.I.DisposeScene();
		base.OnUnload();
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);

		// Update the opengl viewport
		//GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

		// Tell ImGui of the new size
		ImGuiController?.WindowResized(ClientSize.X, ClientSize.Y);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		Debug.StartTimer("Scene Update");
		Scene.I.Update();
		Debug.EndTimer("Scene Update");

		if (Global.EditorAttached)
		{
			Editor.I.Update();
		}

		base.OnUpdateFrame(args);
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		Debug.CountStat("Draw Calls", 0);

		Debug.StartTimer("Scene Render");

		RenderPassSystem.RenderAllPasses();

		Debug.EndTimer("Scene Render");


		Debug.StartTimer("ImGui");

		ImGuiController.Update(this, (float) e.Time);
		GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

		ImGuiController.WindowResized(ClientSize.X, ClientSize.Y);


		Editor.I.Draw();
		//GL.Enable(EnableCap.Multisample);

		ImGuiController.Render();
		Debug.EndTimer("ImGui");

		// ------------- IMGUI -------------


		SwapBuffers();
		base.OnRenderFrame(e);

		Debug.ClearTimers();
		Debug.ClearStats();
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