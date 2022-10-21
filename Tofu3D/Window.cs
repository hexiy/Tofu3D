using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Tofu3D;

public class Window : GameWindow
{
	public RenderTexture BloomDownscaledRenderTexture;
	public ImGuiController ImGuiController;
	public RenderTexture PostProcessRenderTexture;
	public RenderTexture SceneRenderTexture;

	public Window() : base(GameWindowSettings.Default,
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

		//MaterialCache.CacheAllMaterialsInProject();
		ImGuiController = new ImGuiController(ClientSize.X, ClientSize.Y);

		Vector2 size = new(100, 100); // temporaly 10x10 textures because we cant access Camera.I.size before Scene started-camera is a gameobject
		SceneRenderTexture = new RenderTexture(size);
		PostProcessRenderTexture = new RenderTexture(size);

		Editor.I.Init();
		Scene.I.Start();
		SceneRenderTexture = new RenderTexture(Camera.I.Size);
		PostProcessRenderTexture = new RenderTexture(Camera.I.Size);

		//bloomDownscaledRenderTexture = new RenderTexture(Camera.I.size);
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

		GL.ClearColor(0, 0, 0, 0);
		GL.Clear(ClearBufferMask.ColorBufferBit);

		SceneRenderTexture.Bind(); // start rendering to sceneRenderTexture
		GL.Viewport(0, 0, (int) Camera.I.Size.X, (int) Camera.I.Size.Y);

		GL.Enable(EnableCap.Blend);
		//GL.Enable(EnableCap.Multisample);
		Scene.I.Render();

		SceneRenderTexture.Unbind(); // end rendering to sceneRenderTexture
		GL.Disable(EnableCap.Blend);

		PostProcessRenderTexture.Bind();
		GL.ClearColor(0, 0, 0, 0);
		GL.Clear(ClearBufferMask.ColorBufferBit);

		// draw sceneRenderTexture.colorAttachment with post process- into postProcessRenderTexture target
		PostProcessRenderTexture.Render(SceneRenderTexture.ColorAttachment);

		//postProcessRenderTexture.RenderWithPostProcess(sceneRenderTexture.colorAttachment);
		//postProcessRenderTexture.RenderSnow(sceneRenderTexture.colorAttachment);

		PostProcessRenderTexture.Unbind();


		ImGuiController.Update(this, (float) e.Time);
		GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

		ImGuiController.WindowResized(ClientSize.X, ClientSize.Y);


		Editor.I.Draw();
		//GL.Enable(EnableCap.Multisample);

		ImGuiController.Render();

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