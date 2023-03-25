using OpenTK.Windowing.Common.Input;
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

	public static bool AllowPassThroughEdges = false;

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

		bool passedThroughEdge = false;
		if (AllowPassThroughEdges)
		{
			if (state.Position.X == 0 && state.PreviousPosition.X > 0)
			{
				Tofu.I.Window.MousePosition = new OpenTK.Mathematics.Vector2(Tofu.I.Window.Size.X - 2, Tofu.I.Window.MousePosition.Y);
				passedThroughEdge = true;
			}
			// do what the if statement above does but for the right side of the screen
			else if (state.Position.X == Tofu.I.Window.Size.X - 1 && state.PreviousPosition.X < Tofu.I.Window.Size.X)
			{
				Tofu.I.Window.MousePosition = new OpenTK.Mathematics.Vector2(0 + 2, Tofu.I.Window.MousePosition.Y);
				passedThroughEdge = true;
			}

			if (passedThroughEdge)
			{
				state = Tofu.I.Window.MouseState;
			}
		}


		ScreenDelta = new Vector2(state.Delta.X, -state.Delta.Y);

		if (passedThroughEdge || Math.Abs(ScreenDelta.X) > Tofu.I.Window.Size.X-10 || Math.Abs(ScreenDelta.Y) > Tofu.I.Window.Size.Y-10)
		{
			ScreenDelta = Vector2.Zero;
		}

		
		// if (Camera.I.IsOrthographic == false)
		// {
		// 	ScreenDelta = new Vector2(state.Delta.X, -state.Delta.Y) * Global.EditorScale / Units.OneWorldUnit;
		// }

		ScreenPosition = new Vector2(Tofu.I.Window.MouseState.X - Editor.SceneViewPosition.X,
		                             -Tofu.I.Window.MouseState.Y + Camera.I.Size.Y + Editor.SceneViewPosition.Y);

		// Debug.ClearLogs();
		// Debug.Log($"ScreenPos: [{(int)ScreenPosition.X}:{(int)ScreenPosition.Y}]");
		//Debug.Log($"WorldPos: [{(int)WorldPosition.X}:{(int)WorldPosition.Y}]");

		//System.Diagnostics.Debug.WriteLine("mousePos:" + Position.X + ":" + Position.Y);
	}

	static float _sceneViewPadding = 20;

	static bool IsMouseInSceneView()
	{
		return ScreenPosition.X > -_sceneViewPadding && ScreenPosition.X < Camera.I.Size.X + _sceneViewPadding && ScreenPosition.Y > -_sceneViewPadding && ScreenPosition.Y < Camera.I.Size.Y + _sceneViewPadding;
	}
}