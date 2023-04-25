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

	public static Vector2 ScreenDelta { get; private set; }

	/// <summary>
	/// Screen position of mouse
	/// </summary>
	public static Vector2 ScreenPosition { get; private set; } = Vector2.Zero;

	private static List<Func<bool>> _passThroughEdgesConditions = new List<Func<bool>>();

	public static void RegisterPassThroughEdgesCondition(Func<bool> func)
	{
		_passThroughEdgesConditions.Add(func);
	}

	/// <summary>
	/// Returns true if any condition returns true
	/// </summary>
	/// <returns></returns>
	private static bool EvaluateAllPassThroughEdgesConditions()
	{
		foreach (Func<bool> condition in _passThroughEdgesConditions)
		{
			if (condition.Invoke() == true)
			{
				return true;
			}
		}

		return false;
	}

	// public static EventHandler<Func<bool>> PassThroughEdgesConditions;

	public static Vector2 WorldDelta
	{
		get
		{
			if (Camera.MainCamera.IsOrthographic)
			{
				return ScreenDelta * Camera.MainCamera.OrthographicSize / Units.OneWorldUnit;
			}
			else
			{
				return ScreenDelta / Units.OneWorldUnit;
			}
		}
	}

	public static Vector2 WorldPosition
	{
		get { return Camera.MainCamera.ScreenToWorld(ScreenPosition); }
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

	static bool _skipOneFrame = false;

	public static void Update()
	{
		if (_skipOneFrame)
		{
			_skipOneFrame = false;
			return;
		}

		bool allowPassThroughEdges = EvaluateAllPassThroughEdgesConditions(); // uh so how does this work, do i get true when all of them are true or what

		// Debug.StatSetValue("MouseInput AllowPassthroughEdges", $"AllowPassthroughEdges {allowPassThroughEdges}");
		MouseState mouseState = Tofu.I.Window.MouseState;
		Vector2 mousePosCorrected = new Vector2(mouseState.Position.X, Tofu.I.Window.Size.Y - mouseState.Position.Y);
		// Debug.StatSetValue("mousePos", $"MousePos:{mousePosCorrected}");
		bool passedThroughEdge = false;
		if (allowPassThroughEdges)
		{
			if (mousePosCorrected.X < 1 && ScreenDelta.X < 0)
			{
				Tofu.I.Window.MousePosition = new OpenTK.Mathematics.Vector2(Tofu.I.Window.Size.X - 5, Tofu.I.Window.MousePosition.Y);
				passedThroughEdge = true;
			}
			// do what the if statement above does but for the right side of the screen
			else if (mousePosCorrected.X > Tofu.I.Window.Size.X - 2 && ScreenDelta.X > 0)
			{
				Tofu.I.Window.MousePosition = new OpenTK.Mathematics.Vector2(5, Tofu.I.Window.MousePosition.Y);
				passedThroughEdge = true;
			}
			else if (mousePosCorrected.Y > Tofu.I.Window.Size.Y - 2 && ScreenDelta.Y > 0)
			{
				Tofu.I.Window.MousePosition = new OpenTK.Mathematics.Vector2(Tofu.I.Window.MousePosition.X, Tofu.I.Window.Size.Y);
				passedThroughEdge = true;
			}
			else if (mousePosCorrected.Y < 1 && ScreenDelta.Y < 0)
			{
				Tofu.I.Window.MousePosition = new OpenTK.Mathematics.Vector2(Tofu.I.Window.MousePosition.X, 5);
				passedThroughEdge = true;
			}


			if (passedThroughEdge)
			{
				mouseState = Tofu.I.Window.MouseState;
			}
		}

		ScreenDelta = new Vector2(mouseState.Delta.X, -mouseState.Delta.Y);

		if (passedThroughEdge || Math.Abs(ScreenDelta.X) > Tofu.I.Window.Size.X - 100 || Math.Abs(ScreenDelta.Y) > Tofu.I.Window.Size.Y - 100)
		{
			_skipOneFrame = true;
			// Debug.Log($"ScreenDelta:{ScreenDelta.ToString()}");
			ScreenDelta = Vector2.Zero;
		}


		// if (Camera.I.IsOrthographic == false)
		// {
		// 	ScreenDelta = new Vector2(state.Delta.X, -state.Delta.Y) * Global.EditorScale / Units.OneWorldUnit;
		// }

		ScreenPosition = new Vector2(Tofu.I.Window.MouseState.X - Editor.SceneViewPosition.X,
		                             -Tofu.I.Window.MouseState.Y + Camera.MainCamera.Size.Y + Editor.SceneViewPosition.Y + 25); // 25 EditorPanelMenuBar height
	}

	static float _sceneViewPadding = 20;

	static bool IsMouseInSceneView()
	{
		return ScreenPosition.X > -_sceneViewPadding && ScreenPosition.X < Camera.MainCamera.Size.X + _sceneViewPadding && ScreenPosition.Y > -_sceneViewPadding && ScreenPosition.Y < Camera.MainCamera.Size.Y + _sceneViewPadding;
	}
}