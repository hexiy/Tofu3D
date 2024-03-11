using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Tofu3D;

public partial class MouseInput
{
    public delegate void MouseEvent();

    //
    // Summary:
    //     Specifies the buttons of a mouse.
    private float _sceneViewPadding = 20;

    public Vector2 ScreenDelta { get; private set; }

    /// <summary>
    /// Screen position of mouse
    /// </summary>
    public Vector2 PositionInView { get; private set; } = Vector2.Zero;

    public Vector2 PositionInWindow { get; private set; } = Vector2.Zero;

    private List<Func<bool>> _passThroughEdgesConditions = new();

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
        foreach (var condition in _passThroughEdgesConditions)
            if (condition.Invoke() == true)
                return true;

        return false;
    }

    // public static EventHandler<Func<bool>> PassThroughEdgesConditions;

    public Vector2 WorldDelta
    {
        get
        {
            if (Camera.MainCamera.IsOrthographic)
                return ScreenDelta * Camera.MainCamera.OrthographicSize;
            else
                return ScreenDelta;
        }
    }

    public Vector2 WorldPosition => Camera.MainCamera.ScreenToWorld(PositionInView);

    public float ScrollDelta
    {
        get
        {
            if (IsMouseInSceneView == false) return 0;

            return Tofu.Window.MouseState.ScrollDelta.Y;
        }
    }

    public bool IsButtonDown(MouseButtons mouseButton = MouseButtons.Left)
    {
        // if (IsMouseInSceneView() == false)
        // {
        // 	return false;
        // }

        return Tofu.Window.MouseState.IsButtonDown((MouseButton)mouseButton);
    }

    public bool IsButtonUp(MouseButtons mouseButton = MouseButtons.Left)
    {
        if (IsMouseInSceneView == false) return false;

        return Tofu.Window.MouseState.IsButtonDown((MouseButton)mouseButton) == false;
    }

    public bool ButtonPressed(MouseButtons mouseButton = MouseButtons.Left)
    {
        // if (IsMouseInSceneView() == false)
        // {
        // 	return false;
        // }

        return Tofu.Window.MouseState.WasButtonDown((MouseButton)mouseButton) == false &&
               Tofu.Window.MouseState.IsButtonDown((MouseButton)mouseButton);
    }

    public bool ButtonReleased(MouseButtons mouseButton = MouseButtons.Left)
    {
        // if (IsMouseInSceneView() == false) commented 2023/05/04
        // {
        // 	return false;
        // }

        return Tofu.Window.MouseState.WasButtonDown((MouseButton)mouseButton) &&
               Tofu.Window.MouseState.IsButtonDown((MouseButton)mouseButton) == false;
    }

    private bool _skipOneFrame = false;

    public void Update()
    {
        if (_skipOneFrame)
        {
            _skipOneFrame = false;
            return;
        }

        bool allowPassThroughEdges =
            EvaluateAllPassThroughEdgesConditions(); // uh so how does this work, do i get true when all of them are true or what

        // Debug.StatSetValue("MouseInput AllowPassthroughEdges", $"AllowPassthroughEdges {allowPassThroughEdges}");
        MouseState mouseState = Tofu.Window.MouseState;
        Vector2 mousePosCorrected = new(mouseState.Position.X, Tofu.Window.Size.Y - mouseState.Position.Y);
        // Debug.StatSetValue("mousePos", $"MousePos:{mousePosCorrected}");
        bool passedThroughEdge = false;
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


            if (passedThroughEdge) mouseState = Tofu.Window.MouseState;
        }

        ScreenDelta = new Vector2(mouseState.Delta.X, -mouseState.Delta.Y);

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


        PositionInWindow = ImGuiHelper.FlipYToGoodSpace(Tofu.Window.MousePosition);
        PositionInView = new Vector2(PositionInWindow.X- Tofu.Editor.SceneViewPosition.X,
            PositionInWindow.Y - Tofu.Editor.SceneViewPosition.Y);

        Debug.StatSetValue("MousePos",$"Mouse Position In Editor:{PositionInWindow}");
        Debug.StatSetValue("PositionInView",$"PositionInView:{PositionInView}");
        Debug.StatSetValue("SceneViewPos",$"SceneViewPos:{Tofu.Editor.SceneViewPosition}");
        // Debug.StatSetValue("MousePosFlipped",$"mouse pos flipped:{ImGuiHelper.FlipYToGoodSpace(Tofu.Window.MousePosition)}");
        // Debug.StatSetValue("Mouse position editor", $"Mouse pos in editor: {PositionInWindow}");
        // Debug.StatSetValue("Mouse position editor", $"Mouse pos in editor: {PositionInWindow}");
        // Debug.StatSetValue("Mouse position scene view", $"Mouse pos in scene view: {PositionInView}");
        // Debug.StatSetValue("SceneViewPosition", $"sceneViewPosition: {Tofu.Editor.SceneViewPosition}");
        // Debug.StatSetValue("imgui mouse pos", $"imgui mmouse pos: {ImGui.GetMousePos()}");
    }


    public bool IsMouseInSceneView = false;
}