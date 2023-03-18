using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Tofu3D;

public static class MouseInput
{
	public delegate void MouseEvent();

	//
	// Summary:
	//     Specifies the buttons of a mouse.
	public enum Buttons
	{
		//
		// Summary:
		//     The first button.
		Button1 = 0,

		//
		// Summary:
		//     The second button.
		Button2 = 1,

		//
		// Summary:
		//     The third button.
		Button3 = 2,

		//
		// Summary:
		//     The fourth button.
		Button4 = 3,

		//
		// Summary:
		//     The fifth button.
		Button5 = 4,

		//
		// Summary:
		//     The sixth button.
		Button6 = 5,

		//
		// Summary:
		//     The seventh button.
		Button7 = 6,

		//
		// Summary:
		//     The eighth button.
		Button8 = 7,

		//
		// Summary:
		//     The left mouse button. This corresponds to OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button1.
		Left = 0,

		//
		// Summary:
		//     The right mouse button. This corresponds to OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button2.
		Right = 1,

		//
		// Summary:
		//     The middle mouse button. This corresponds to OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button3.
		Middle = 2,

		//
		// Summary:
		//     The highest mouse button available.
		Last = 7
	}

	public static Vector2 ScreenDelta;

	/// <summary>
	///         Screen position of mouse
	/// </summary>
	public static Vector2 ScreenPosition = Vector2.Zero;

	public static Vector2 WorldDelta
	{
		get
		{
			if (Camera.I.IsOrthographic)
			{
				return ScreenDelta * Camera.I.OrthographicSize / Units.OneWorldUnit;
			}
			else
			{
				return ScreenDelta / Units.OneWorldUnit;
			}
		}
	}

	public static Vector2 WorldPosition
	{
		get { return Camera.I.ScreenToWorld(ScreenPosition); }
	}

	public static float ScrollDelta
	{
		get
		{
			if (IsMouseInSceneView() == false)
			{
				return 0;
			}

			return Tofu.I.Window.MouseState.ScrollDelta.Y;
		}
	}

	public static bool IsButtonDown(Buttons button = Buttons.Left)
	{
		// if (IsMouseInSceneView() == false)
		// {
		// 	return false;
		// }

		return Tofu.I.Window.MouseState.IsButtonDown((MouseButton) button);
	}

	public static bool IsButtonUp(Buttons button = Buttons.Left)
	{
		if (IsMouseInSceneView() == false)
		{
			return false;
		}

		return Tofu.I.Window.MouseState.IsButtonDown((MouseButton) button) == false;
	}

	public static bool ButtonPressed(Buttons button = Buttons.Left)
	{
		// if (IsMouseInSceneView() == false)
		// {
		// 	return false;
		// }

		return Tofu.I.Window.MouseState.WasButtonDown((MouseButton) button) == false && Tofu.I.Window.MouseState.IsButtonDown((MouseButton) button);
	}

	public static bool ButtonReleased(Buttons button = Buttons.Left)
	{
		if (IsMouseInSceneView() == false)
		{
			return false;
		}

		return Tofu.I.Window.MouseState.WasButtonDown((MouseButton) button) && Tofu.I.Window.MouseState.IsButtonDown((MouseButton) button) == false;
	}

	public static void Update()
	{
		MouseState state = Tofu.I.Window.MouseState;

		ScreenDelta = new Vector2(state.Delta.X, -state.Delta.Y) * Global.EditorScale;
		// if (Camera.I.IsOrthographic == false)
		// {
		// 	ScreenDelta = new Vector2(state.Delta.X, -state.Delta.Y) * Global.EditorScale / Units.OneWorldUnit;
		// }

		ScreenPosition = new Vector2(Tofu.I.Window.MouseState.X * Global.EditorScale - Editor.SceneViewPosition.X,
		                             -Tofu.I.Window.MouseState.Y * Global.EditorScale + Camera.I.Size.Y + Editor.SceneViewPosition.Y);

		//Debug.Log($"ScreenPos: [{(int)ScreenPosition.X}:{(int)ScreenPosition.Y}]");
		//Debug.Log($"WorldPos: [{(int)WorldPosition.X}:{(int)WorldPosition.Y}]");

		//System.Diagnostics.Debug.WriteLine("mousePos:" + Position.X + ":" + Position.Y);
	}

	static float _sceneViewPadding = 20;
	static bool IsMouseInSceneView()
	{
		return ScreenPosition.X < Camera.I.Size.X-_sceneViewPadding && ScreenPosition.Y < Camera.I.Size.Y-_sceneViewPadding && ScreenPosition.X > 0+_sceneViewPadding && ScreenPosition.Y > 0+_sceneViewPadding;
	}
}