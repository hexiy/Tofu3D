using Engine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Tofu3D.Rendering;

namespace Tofu3D;

public class Window : GameWindow
{
	public ImGuiController ImGuiController;

	public Window() : base(new GameWindowSettings() { }, // dont specify fps.... otherwise deltatime fucks up and update and render is called not 1:1
	                       new NativeWindowSettings
	                       {
		                       /*Size = new Vector2i(2560, 1600),*/ APIVersion = new Version(4, 1), Flags = ContextFlags.ForwardCompatible, Profile = ContextProfile.Core, /*NumberOfSamples = 8,*/ StartFocused = true,
	                       })
	{
		I = this;
		VSync = VSyncMode.Off;
		this.UpdateFrequency = 500;
		this.RenderFrequency = 0;
		WindowState = WindowState.Maximized;
		// WindowState = WindowState.Fullscreen;
		Title = WindowTitleText;
	}

	public static Window I { get; private set; }

	public string WindowTitleText
	{
		get { return $"Tofu3D | {GL.GetString(StringName.Version)}"; }
	}

	protected override void OnLoad()
	{
		ImGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);

		Editor.I.Init();
		Scene.I.Start();

		// RenderPassSystem.Initialize();
		// MousePickingSystem.Initialize();
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

	protected override void OnUpdateFrame(FrameEventArgs e)
	{
		/*_updatesCalled++;
		_elapsedTime += (float)e.Time;
		if (_elapsedTime >= 1 )
		{
			Debug.Log($"Updates in 1 second:{_updatesCalled}");
			_updatesCalled = 0;
			_elapsedTime = 0;
			Time.EditorElapsedTime = 0;
		}*/

		// Time.DeltaTimeUpdate = (float) e.Time;
		// Title = (1f / Time.DeltaTimeUpdate).ToString("F2");

		// Time.StartDeltaTimeUpdateStopWatch();
		Debug.StartGraphTimer("Scene Update", DebugGraphTimer.SourceGroup.Update, TimeSpan.FromSeconds(1f / 60f));
		Scene.I.Update();
		Debug.EndGraphTimer("Scene Update");

		if (Global.EditorAttached)
		{
			// Debug.StartTimer("Editor Update", DebugTimer.SourceGroup.Update, TimeSpan.FromMilliseconds(1f / 60f));
			Editor.I.Update();
			// Debug.EndTimer("Editor Update");
		}

		base.OnUpdateFrame(e);

		// Time.EndDeltaTimeUpdateStopWatch();
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		Time.EditorDeltaTime = (float) (e.Time);

		// Time.StartDeltaTimeRenderStopWatch();

		Debug.StartGraphTimer("Window Render", DebugGraphTimer.SourceGroup.Render, TimeSpan.FromSeconds(1 / 60f), -1);

		Debug.StatSetAdditiveValue("Draw Calls", 0);
		// Debug.StartTimer("Test", DebugTimer.SourceGroup.Gpu, TimeSpan.FromSeconds(1f / 60f));
		// Debug.EndTimer("Test");

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

			Editor.I.Draw();
			//GL.Enable(EnableCap.Multisample);

			ImGuiController.Render();
		}

		Debug.EndGraphTimer("ImGui");

		// ------------- IMGUI -------------


		SwapBuffers();

		base.OnRenderFrame(e);

		Debug.EndGraphTimer("Window Render");

		// Time.EndDeltaTimeRenderStopWatch();

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