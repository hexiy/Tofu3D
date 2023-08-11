using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Tofu3D;

public partial class MouseInput
{
	public delegate void MouseEvent();

	//
	// Summary:
	//     Specifies the buttons of a mouse.
	float _sceneViewPadding = 20;

	public Vector2 ScreenDelta { get; private set; }

	/// <summary>
	/// Screen position of mouse
	/// </summary>
	public Vector2 ScreenPosition { get; private set; } = Vector2.Zero;

	private List<Func<bool>> _passThroughEdgesConditions = new List<Func<bool>>();

	public void RegisterPassThroughEdgesCondition(Func<bool> func)
	{
		_passThroughEdgesConditions.Add(func);
	}

	/// <summary>
	/// Returns true if any condition returns true
	/// </summary>
	/// <returns></returns>
	private bool EvaluateAllPassThroughEdgesConditions()
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

	public Vector2 WorldDelta
	{
		get
		{
			if (Camera.MainCamera.IsOrthographic)
			{
				return ScreenDelta * Camera.MainCamera.OrthographicSize;
			}
			else
			{
				return ScreenDelta;
			}
		}
	}

	public Vector2 WorldPosition
	{
		get { return Camera.MainCamera.ScreenToWorld(ScreenPosition); }
	}

	public float ScrollDelta
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

	public bool IsButtonDown(MouseButtons mouseButton = MouseButtons.Left)
	{
		// if (IsMouseInSceneView() == false)
		// {
		// 	return false;
		// }

		return Tofu.I.Window.MouseState.IsButtonDown((MouseButton) mouseButton);
	}

	public bool IsButtonUp(MouseButtons mouseButton = MouseButtons.Left)
	{
		if (IsMouseInSceneView() == false)
		{
			return false;
		}

		return Tofu.I.Window.MouseState.IsButtonDown((MouseButton) mouseButton) == false;
	}

	public bool ButtonPressed(MouseButtons mouseButton = MouseButtons.Left)
	{
		// if (IsMouseInSceneView() == false)
		// {
		// 	return false;
		// }

		return Tofu.I.Window.MouseState.WasButtonDown((MouseButton) mouseButton) == false && Tofu.I.Window.MouseState.IsButtonDown((MouseButton) mouseButton);
	}

	public bool ButtonReleased(MouseButtons mouseButton = MouseButtons.Left)
	{
		// if (IsMouseInSceneView() == false) commented 2023/05/04
		// {
		// 	return false;
		// }

		return Tofu.I.Window.MouseState.WasButtonDown((MouseButton) mouseButton) && Tofu.I.Window.MouseState.IsButtonDown((MouseButton) mouseButton) == false;
	}

    bool _skipOneFrame = false;

	public void Update()
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

		ScreenPosition = new Vector2(Tofu.I.Window.MouseState.X - Tofu.I.Editor.SceneViewPosition.X,
		                             -Tofu.I.Window.MouseState.Y + Camera.MainCamera.Size.Y + Tofu.I.Editor.SceneViewPosition.Y + 25); // 25 EditorPanelMenuBar height
	}


    bool IsMouseInSceneView()
	{
		return ScreenPosition.X > -_sceneViewPadding && ScreenPosition.X < Camera.MainCamera.Size.X + _sceneViewPadding && ScreenPosition.Y > -_sceneViewPadding && ScreenPosition.Y < Camera.MainCamera.Size.Y + _sceneViewPadding;
	}
}