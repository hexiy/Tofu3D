using Tofu3D.Tweening;

namespace Tofu3D;

public class SceneViewController
{
    private readonly PersistentObject<float> _cameraFieldOfViewInperspectiveMode =
        ("_cameraFieldOfViewInperspectiveMode", 90);

    private readonly PersistentObject<Vector3> _cameraPositionInPerspectiveMode =
        ("_cameraPositionInPerspectiveMode", Vector3.Zero);

    // rotation before going into orthographic mode
    private readonly PersistentObject<Vector3> _cameraRotationInPerspectiveMode =
        ("_cameraRotationInPerspectiveMode", Vector3.Zero);

    private bool _clickedInsideScene;

    private Vector3 _keyboardInputDirectionVector = Vector3.Zero;
    private readonly float _moveSpeed = 5f; // WASD units moved per seconds
    private float _moveSpeedMultiplier = 1; // WASD units moved per seconds

    private Vector3 _smoothKeyboardInputMoveVector = Vector3.Zero;

    // PersistentObject<int> _savedInt = new PersistentObject<int>();
    private Vector2 _smoothScreenDeltaVectorForMovement = Vector3.Zero;
    private Vector2 _smoothScreenDeltaVectorForRotation = Vector3.Zero;
    private float _targetOrthoSize = -1;

    public PersistentObject<ProjectionMode> CurrentProjectionMode =
        ("sceneViewProjectionMode", ProjectionMode.Perspective);

    public SceneViewController()
    {
        Tofu.MouseInput.RegisterPassThroughEdgesCondition(() => AllowPassThroughEdges);
        SetProjectionMode(CurrentProjectionMode);
    }

    public bool IsPanningCamera { get; private set; }

    public bool AllowPassThroughEdges { get; set; }
    // public ProjectionMode ProjectionMode
    // {
    // 	get { return (ProjectionMode) PersistentData.GetInt("SceneViewControllerPerspectiveMode", 0); }
    // 	private set { PersistentData.Set("SceneViewControllerPerspectiveMode", (int) value); }
    // }

    public void MoveToGameObject(GameObject targetGo)
    {
        var cameraStartPos = Camera.MainCamera.Transform.LocalPosition;
        var cameraEndPos = targetGo.Transform.LocalPosition + new Vector3(0, 0, -4);

        if (cameraStartPos == cameraEndPos)
        {
            cameraEndPos = targetGo.Transform.LocalPosition + new Vector3(0, 0, -2);
        }

        var cameraOrthoSize = Camera.MainCamera.OrthographicSize;
        Tweener.Tween(0, 1, 1.3f, progress =>
        {
            // Debug.Log("TWEENING:" + progress);
            Camera.MainCamera.OrthographicSize = cameraOrthoSize + (float)MathHelper.Sin(progress * Mathf.Pi) * 0.8f;
            Camera.MainCamera.Transform.LocalPosition = Vector3.Lerp(cameraStartPos, cameraEndPos, progress);
        });
    }

    public void SetProjectionMode(ProjectionMode newProjectionMode)
    {
        if (newProjectionMode == CurrentProjectionMode)
        {
            return;
        }

        if (newProjectionMode == ProjectionMode.Orthographic && CurrentProjectionMode == ProjectionMode.Perspective)
        {
            _cameraRotationInPerspectiveMode.Value = Camera.MainCamera.Transform.Rotation;
            _cameraPositionInPerspectiveMode.Value = Camera.MainCamera.Transform.WorldPosition;
            _cameraFieldOfViewInperspectiveMode.Value = Camera.MainCamera.FieldOfView;
        }

        var tweenDuration = 1f;
        Tweener.Tween(Camera.MainCamera.Transform.Rotation.X,
            newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.X : 0,
            tweenDuration,
            f => { Camera.MainCamera.Transform.Rotation = Camera.MainCamera.Transform.Rotation.Set(f); });
        Tweener.Tween(Camera.MainCamera.Transform.Rotation.Y,
            newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.Y : 0,
            tweenDuration,
            f => { Camera.MainCamera.Transform.Rotation = Camera.MainCamera.Transform.Rotation.Set(y: f); });
        Tweener.Tween(Camera.MainCamera.Transform.WorldPosition.Z,
            newProjectionMode == ProjectionMode.Perspective
                ? _cameraPositionInPerspectiveMode.Value.Z
                : Camera.MainCamera.Transform.WorldPosition.Z - 350, tweenDuration,
            f => { Camera.MainCamera.Transform.WorldPosition = Camera.MainCamera.Transform.WorldPosition.Set(z: f); });

        var tween = Tweener.Tween(Camera.MainCamera.Transform.Rotation.Z,
            newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.Z : 0,
            tweenDuration,
            f => { Camera.MainCamera.Transform.Rotation = Camera.MainCamera.Transform.Rotation.Set(z: f); });

        Tweener.Tween(Camera.MainCamera.FieldOfView,
            newProjectionMode == ProjectionMode.Perspective ? _cameraFieldOfViewInperspectiveMode : 14, tweenDuration,
            f => { Camera.MainCamera.FieldOfView = f; });


        if (newProjectionMode == ProjectionMode.Orthographic)
        {
            tween.SetOnComplete(() =>
            {
                Camera.MainCamera.IsOrthographic = newProjectionMode == ProjectionMode.Orthographic;
            });
        }
        else
        {
            Camera.MainCamera.IsOrthographic = newProjectionMode == ProjectionMode.Orthographic;
        }

        CurrentProjectionMode.Value = newProjectionMode;
    }

    private string isMouseOverSceneViewStringYes = $"isMouseOverSceneView:yes";
    private string isMouseOverSceneViewStringNo = $"isMouseOverSceneView:no";
    private float _mouseSensitivity = 0.2f;

    public void Update()
    {
        IsPanningCamera = false;
        if (_targetOrthoSize == -1 && Camera.MainCamera != null)
        {
            _targetOrthoSize = Camera.MainCamera.OrthographicSize;
        }

        if (TransformHandle.I.Clicked)
        {
            //return;
        }

        if (Global.EditorAttached == false)
        {
            return;
        }

        // if (KeyboardInput.WasKeyJustPressed(Keys.F))
        // {
        //     Debug.Log("TODO");
        // }
        // todo MoveToGameObject(GameObjectSelectionManager.GetSelectedGameObject());

        var isMouseOverSceneView = Tofu.MouseInput.IsMouseInSceneView;
// Debug.Log($"isMouseOverSceneView:{isMouseOverSceneView}");
        Debug.StatSetValue("isMouseOverSceneView",
            isMouseOverSceneView ? isMouseOverSceneViewStringYes : isMouseOverSceneViewStringNo);
        var justClicked = Tofu.MouseInput.ButtonPressed() |
                          Tofu.MouseInput.ButtonPressed(MouseButtons.Right);
        if (justClicked)
        {
            _clickedInsideScene = isMouseOverSceneView;
        }

        AllowPassThroughEdges = false;

        if (isMouseOverSceneView)
        {
            HandleMouseScroll();
        }

        var validInput = ((isMouseOverSceneView || _clickedInsideScene) && _clickedInsideScene) ||
                         (justClicked == false && isMouseOverSceneView && _clickedInsideScene);
        if (Tofu.MouseInput.IsButtonDown() || Tofu.MouseInput.IsButtonDown(MouseButtons.Right))
        {
            if (validInput)
            {
                AllowPassThroughEdges = true;

                HandleButtonInputs();
            }
        }

        if (Camera.MainCamera.IsOrthographic == false)
        {
            if (validInput && Tofu.MouseInput.IsButtonDown(MouseButtons.Right))
            {
                _smoothScreenDeltaVectorForMovement = Vector2.Lerp(_smoothScreenDeltaVectorForMovement,
                    Tofu.MouseInput.ScreenDelta, Time.EditorDeltaTime * 15);
            }
            else
            {
                _smoothScreenDeltaVectorForMovement = Vector2.Lerp(_smoothScreenDeltaVectorForMovement, Vector2.Zero,
                    Time.EditorDeltaTime * 7);
            }

            if (validInput && Tofu.MouseInput.IsButtonDown())
            {
                _smoothScreenDeltaVectorForRotation = Tofu.MouseInput.ScreenDelta * _mouseSensitivity; // instant

                // _smoothScreenDeltaVectorForRotation = Vector2.Lerp(_smoothScreenDeltaVectorForRotation,
                    // Tofu.MouseInput.ScreenDelta * _mouseSensitivity, 0.1f);
            }
            else
            {
                _smoothScreenDeltaVectorForRotation = Vector3.Zero; // instant

                // _smoothScreenDeltaVectorForRotation = Vector2.Lerp(_smoothScreenDeltaVectorForRotation,
                    // Vector2.Zero, 0.12f);
            }

            MoveCameraByLocalVector(_smoothScreenDeltaVectorForMovement * 30 / Tofu.Window.WindowSize);

            Camera.MainCamera.Transform.Rotation += new Vector3(-_smoothScreenDeltaVectorForRotation.Y,
                _smoothScreenDeltaVectorForRotation.X, 0);
            // Camera.MainCamera.Transform.Rotation += new Vector3(-_smoothScreenDeltaVectorForRotation.Y,
            // _smoothScreenDeltaVectorForRotation.X, 0) * 1000 * Time.EditorDeltaTime;

            var keyboardMoveSpeed = _moveSpeed;

            _keyboardInputDirectionVector = Vector3.Zero;
            if (KeyboardInput.IsKeyDown(Keys.LeftControl) == false)
            {
                if (KeyboardInput.IsKeyDown(Keys.W))
                {
                    _keyboardInputDirectionVector += Vector3.Forward;
                }

                if (KeyboardInput.IsKeyDown(Keys.S))
                {
                    _keyboardInputDirectionVector += Vector3.Backward;
                }

                if (KeyboardInput.IsKeyDown(Keys.A))
                {
                    _keyboardInputDirectionVector += Vector3.Left;
                }

                if (KeyboardInput.IsKeyDown(Keys.D))
                {
                    _keyboardInputDirectionVector += Vector3.Right;
                }
            }


            if (KeyboardInput.IsKeyDown(Keys.LeftShift))
            {
                _moveSpeedMultiplier = Mathf.Lerp(_moveSpeedMultiplier, 2, Time.EditorDeltaTime * 10);
            }
            //Camera.I.FieldOfView = Mathf.Lerp(Camera.I.FieldOfView, 100, Time.EditorDeltaTime * 7);
            else
            {
                _moveSpeedMultiplier = Mathf.Lerp(_moveSpeedMultiplier, 1, Time.EditorDeltaTime * 10);
            }

            //Camera.I.FieldOfView = Mathf.Lerp(Camera.I.FieldOfView, 60, Time.EditorDeltaTime * 7);
            keyboardMoveSpeed = keyboardMoveSpeed * _moveSpeedMultiplier;

            _smoothKeyboardInputMoveVector = Vector3.Lerp(_smoothKeyboardInputMoveVector, _keyboardInputDirectionVector,
                Time.EditorDeltaTime * 4);

            MoveCameraByLocalVector(_smoothKeyboardInputMoveVector * keyboardMoveSpeed * Time.EditorDeltaTime);
        }
    }

    private void HandleMouseScroll()
    {
        // Z POSITION
        if (Tofu.MouseInput.ScrollDelta != 0)
        {
            if (Camera.MainCamera.IsOrthographic)
            {
                _targetOrthoSize += -Tofu.MouseInput.ScrollDelta * (_targetOrthoSize * 0.04f);
                _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, 0.1f, Mathf.Infinity);
                // Camera.I.ortographicSize = Mathf.Eerp(Camera.I.ortographicSize, targetOrthoSize, Time.editorDeltaTime * 10f);
                // macbook trackpad has smooth scrolling so no eerping
                Camera.MainCamera.OrthographicSize = _targetOrthoSize;
            }
            else
            {
                MoveCameraByLocalVector(new Vector3(0, 0, Mathf.Clamp(Tofu.MouseInput.ScrollDelta, -10, 10)) *
                                        _moveSpeed * 0.2f);

                //Camera.I.transform.position += Camera.I.transform.TransformDirection(Vector3.Forward) * Tofu.MouseInput.ScrollDelta * 0.05f;
            }
        }
    }

    private void HandleButtonInputs()
    {
        // PANNING
        if (Tofu.MouseInput.IsButtonDown() && Camera.MainCamera.IsOrthographic)
        {
            Camera.MainCamera.Transform.LocalPosition +=
                Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(
                    new Vector2(-Tofu.MouseInput.ScreenDelta.X, Tofu.MouseInput.ScreenDelta.Y)) *
                Camera.MainCamera.OrthographicSize;
        }
        // Tofu.MouseInput.ScreenDelta -= Tofu.MouseInput.ScreenDelta;
    }

    private void MoveCameraByLocalVector(Vector3 moveVector)
    {
        var delta = Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(moveVector);

        // Debug.StatSetValue("DASDSAD", $"CameraDir:{Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(moveVector)}");
        //Debug.Log(delta);
        //Camera.I.Transform.LocalPosition += delta;
        Camera.MainCamera.Transform.WorldPosition += delta;
        // Camera.I.Transform.LocalPosition += dir * moveSpeed;

        // Camera.MainCamera.UpdateMatrices();
    }
}