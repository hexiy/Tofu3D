using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Tofu3D;

public class MouseInput
{
    public delegate void MouseEvent();

    private readonly List<Func<bool>> _passThroughEdgesConditions = new();

    //
    // Summary:
    //     Specifies the buttons of a mouse.
    private float _sceneViewPadding = 20;

    private bool _skipOneFrame;


    public bool IsMouseInSceneView = false;

    public Vector2 ScreenDelta { get; private set; }

    /// <summary>
    ///     Screen position of mouse
    /// </summary>
    public Vector2 PositionInView { get; private set; } = Vector2.Zero;

    public Vector2 PositionInWindow { get; private set; } = Vector2.Zero;

    // public static EventHandler<Func<bool>> PassThroughEdgesConditions;

    public Vector2 WorldDelta
    {
        get
        {
            if (Camera.MainCamera.IsOrthographic)
            {
                return ScreenDelta * Camera.MainCamera.OrthographicSize;
            }

            return ScreenDelta;
        }
    }

    public Vector2 WorldPosition => Camera.MainCamera.ScreenToWorld(PositionInView);

    public float ScrollDelta
    {
        get
        {
            if (IsMouseInSceneView == false)
            {
                return 0;
            }

            return Tofu.Window.MouseState.ScrollDelta.Y;
        }
    }

    public void RegisterPassThroughEdgesCondition(Func<bool> func)
    {
        _passThroughEdgesConditions.Add(func);
    }

    /// <summary>
    ///     Returns true if any condition returns true
    /// </summary>
    /// <returns></returns>
    private bool EvaluateAllPassThroughEdgesConditions()
    {
        foreach (var condition in _passThroughEdgesConditions)
        {
            if (condition.Invoke())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsButtonDown(MouseButtons mouseButton = MouseButtons.Left) =>
        // if (IsMouseInSceneView() == false)
        // {
        // 	return false;
        // }
        Tofu.Window.MouseState.IsButtonDown((MouseButton)mouseButton);

    public bool IsButtonUp(MouseButtons mouseButton = MouseButtons.Left)
    {
        if (IsMouseInSceneView == false)
        {
            return false;
        }

        return Tofu.Window.MouseState.IsButtonDown((MouseButton)mouseButton) == false;
    }

    public bool ButtonPressed(MouseButtons mouseButton = MouseButtons.Left) =>
        // if (IsMouseInSceneView() == false)
        // {
        // 	return false;
        // }
        Tofu.Window.MouseState.WasButtonDown((MouseButton)mouseButton) == false &&
        Tofu.Window.MouseState.IsButtonDown((MouseButton)mouseButton);

    public bool ButtonReleased(MouseButtons mouseButton = MouseButtons.Left) =>
        // if (IsMouseInSceneView() == false) commented 2023/05/04
        // {
        // 	return false;
        // }
        // Tofu.Window.MouseState.WasButtonDown((MouseButton)mouseButton) &&
        // Tofu.Window.MouseState.IsButtonDown((MouseButton)mouseButton) == false;
        Tofu.Window.MouseState.IsButtonReleased((MouseButton)mouseButton);

    public void Update()
    {
        if (_skipOneFrame)
        {
            _skipOneFrame = false;
            return;
        }

        var allowPassThroughEdges =
            EvaluateAllPassThroughEdgesConditions(); // uh so how does this work, do i get true when all of them are true or what

        // Debug.StatSetValue("MouseInput AllowPassthroughEdges", $"AllowPassthroughEdges {allowPassThroughEdges}");

        Vector2 oldPosition = PositionInWindow;

        Vector2 pos = new Vector2();
        unsafe
        {
            // GLFW.PollEvents();
            GLFW.GetCursorPos(Tofu.Window.WindowPtr, out double x, out double y);
            pos = new Vector2((float)x, (float)y);
        }

        PositionInWindow = ImGuiHelper.FlipYToGoodSpace(pos);

        ScreenDelta = PositionInWindow - oldPosition;

        Vector2 mousePosCorrected = new(PositionInWindow.X, Tofu.Window.Size.Y - PositionInWindow.Y);
        // Debug.StatSetValue("mousePos", $"MousePos:{mousePosCorrected}");
        var passedThroughEdge = false;
        if (allowPassThroughEdges)
        {
            if (mousePosCorrected.X < 1 && ScreenDelta.X < 0)
            {
                Tofu.Window.MousePosition =
                    new OpenTK.Mathematics.Vector2(Tofu.Window.Size.X - 5, Tofu.Window.MousePosition.Y);
                passedThroughEdge = true;
            }
            // do what the if statement above does but for the right side of the screen
            else if (mousePosCorrected.X > Tofu.Window.Size.X - 2 && ScreenDelta.X > 0)
            {
                Tofu.Window.MousePosition = new OpenTK.Mathematics.Vector2(5, Tofu.Window.MousePosition.Y);
                passedThroughEdge = true;
            }
            else if (mousePosCorrected.Y > Tofu.Window.Size.Y - 2 && ScreenDelta.Y > 0)
            {
                Tofu.Window.MousePosition =
                    new OpenTK.Mathematics.Vector2(Tofu.Window.MousePosition.X, Tofu.Window.Size.Y);
                passedThroughEdge = true;
            }
            else if (mousePosCorrected.Y < 1 && ScreenDelta.Y < 0)
            {
                Tofu.Window.MousePosition = new OpenTK.Mathematics.Vector2(Tofu.Window.MousePosition.X, 5);
                passedThroughEdge = true;
            }


            // if (passedThroughEdge)
            // {
            //     mouseState = Tofu.Window.MouseState;
            // }
        }


        if (passedThroughEdge || Math.Abs(ScreenDelta.X) > Tofu.Window.Size.X - 100 ||
            Math.Abs(ScreenDelta.Y) > Tofu.Window.Size.Y - 100)
        {
            _skipOneFrame = true;
            // Debug.Log($"ScreenDelta:{ScreenDelta.ToString()}");
            ScreenDelta = Vector2.Zero;
        }


        // if (Camera.I.IsOrthographic == false)
        // {
        // 	ScreenDelta = new Vector2(state.Delta.X, -state.Delta.Y) * Global.EditorScale / Units.OneWorldUnit;
        // }


        PositionInView = new Vector2(PositionInWindow.X - Tofu.Editor.SceneViewPosition.X,
            PositionInWindow.Y - Tofu.Editor.SceneViewPosition.Y);

        Debug.StatSetValue("MousePos", $"Mouse Position In Editor:{PositionInWindow}");
        Debug.StatSetValue("PositionInView", $"PositionInView:{PositionInView}");
        Debug.StatSetValue("SceneViewPos", $"SceneViewPos:{Tofu.Editor.SceneViewPosition}");
        Debug.StatSetValue("SceneViewPos", $"SceneViewPos:{Tofu.Editor.SceneViewPosition.X},{Tofu.Editor.SceneViewPosition.Y}");
        // Debug.StatSetValue("MousePosFlipped",$"mouse pos flipped:{ImGuiHelper.FlipYToGoodSpace(Tofu.Window.MousePosition)}");
        // Debug.StatSetValue("Mouse position editor", $"Mouse pos in editor: {PositionInWindow}");
        // Debug.StatSetValue("Mouse position editor", $"Mouse pos in editor: {PositionInWindow}");
        // Debug.StatSetValue("Mouse position scene view", $"Mouse pos in scene view: {PositionInView}");
        // Debug.StatSetValue("SceneViewPosition", $"sceneViewPosition: {Tofu.Editor.SceneViewPosition}");
        // Debug.StatSetValue("imgui mouse pos", $"imgui mmouse pos: {ImGui.GetMousePos()}");
    }
}