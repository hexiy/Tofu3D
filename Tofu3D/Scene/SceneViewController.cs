using Tofu3D.Tweening;

namespace Tofu3D;

public class SceneViewController
{
	bool _clickedInsideScene;
	float _targetOrthoSize = -1;
	float _moveSpeed = 5f; // WASD units moved per seconds
	float _moveSpeedMultiplier = 1; // WASD units moved per seconds

	public SceneViewController()
	{
		I = this;
		MouseInput.RegisterPassThroughEdgesCondition(() => AllowPassThroughEdges);
		SetProjectionMode(CurrentProjectionMode);
	}

	public static SceneViewController I { get; private set; }
	public bool IsPanningCamera { get; private set; }
	public bool AllowPassThroughEdges { get; set; }

	public PersistentObject<ProjectionMode> CurrentProjectionMode = ("sceneViewProjectionMode", Tofu3D.ProjectionMode.Perspective);
	// rotation before going into orthographic mode
	PersistentObject<Vector3> _cameraRotationInPerspectiveMode = ("_cameraRotationInPerspectiveMode", Vector3.Zero);
	PersistentObject<Vector3> _cameraPositionInPerspectiveMode = ("_cameraPositionInPerspectiveMode", Vector3.Zero);
	PersistentObject<float> _cameraFieldOfViewInperspectiveMode = ("_cameraFieldOfViewInperspectiveMode", 90);
	// PersistentObject<int> _savedInt = new PersistentObject<int>();
	Vector2 _smoothScreenDeltaVectorForMovement = Vector3.Zero;
	Vector2 _smoothScreenDeltaVectorForRotation = Vector3.Zero;

	Vector3 _smoothKeyboardInputMoveVector = Vector3.Zero;

	Vector3 _keyboardInputDirectionVector = Vector3.Zero;
	// public ProjectionMode ProjectionMode
	// {
	// 	get { return (ProjectionMode) PersistentData.GetInt("SceneViewControllerPerspectiveMode", 0); }
	// 	private set { PersistentData.Set("SceneViewControllerPerspectiveMode", (int) value); }
	// }

	public void MoveToGameObject(GameObject targetGo)
	{
		Vector3 cameraStartPos = Camera.MainCamera.Transform.LocalPosition;
		Vector3 cameraEndPos = targetGo.Transform.LocalPosition + new Vector3(0, 0, -4);

		if (cameraStartPos == cameraEndPos)
		{
			cameraEndPos = targetGo.Transform.LocalPosition + new Vector3(0, 0, -2);
		}

		float cameraOrthoSize = Camera.MainCamera.OrthographicSize;
		Tweener.Tween(0, 1, 1.3f, progress =>
		{
			// Debug.Log("TWEENING:" + progress);
			Camera.MainCamera.OrthographicSize = cameraOrthoSize + (float) MathHelper.Sin(progress * Mathf.Pi) * 0.8f;
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

		float tweenDuration = 1f;
		Tweener.Tween(Camera.MainCamera.Transform.Rotation.X, newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.X : 0, tweenDuration,
		              (f) => { Camera.MainCamera.Transform.Rotation = Camera.MainCamera.Transform.Rotation.Set(x: f); });
		Tweener.Tween(Camera.MainCamera.Transform.Rotation.Y, newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.Y : 0, tweenDuration,
		              (f) => { Camera.MainCamera.Transform.Rotation = Camera.MainCamera.Transform.Rotation.Set(y: f); });
		Tweener.Tween(Camera.MainCamera.Transform.WorldPosition.Z, newProjectionMode == ProjectionMode.Perspective ? _cameraPositionInPerspectiveMode.Value.Z : Camera.MainCamera.Transform.WorldPosition.Z - 350, tweenDuration,
		              (f) => { Camera.MainCamera.Transform.WorldPosition = Camera.MainCamera.Transform.WorldPosition.Set(z: f); });

		Tween tween = Tweener.Tween(Camera.MainCamera.Transform.Rotation.Z, newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.Z : 0, tweenDuration,
		                            (f) => { Camera.MainCamera.Transform.Rotation = Camera.MainCamera.Transform.Rotation.Set(z: f); });

		Tweener.Tween(Camera.MainCamera.FieldOfView, newProjectionMode == ProjectionMode.Perspective ? _cameraFieldOfViewInperspectiveMode : 14, tweenDuration, (f) => { Camera.MainCamera.FieldOfView = f; });


		if (newProjectionMode == ProjectionMode.Orthographic)
		{
			tween.SetOnComplete(() => { Camera.MainCamera.IsOrthographic = newProjectionMode == ProjectionMode.Orthographic; });
		}
		else
		{
			Camera.MainCamera.IsOrthographic = newProjectionMode == ProjectionMode.Orthographic;
		}

		CurrentProjectionMode.Value = newProjectionMode;
	}

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

		if (KeyboardInput.WasKeyJustPressed(Keys.F))
		{
			Debug.Log("TODO");
			// todo MoveToGameObject(GameObjectSelectionManager.GetSelectedGameObject());
		}


		bool isMouseOverSceneView = MouseInput.ScreenPosition.X < Camera.MainCamera.Size.X && MouseInput.ScreenPosition.Y < Camera.MainCamera.Size.Y && MouseInput.ScreenPosition.Y > 0;
// Debug.Log($"isMouseOverSceneView:{isMouseOverSceneView}");
		bool justClicked = MouseInput.ButtonPressed(MouseInput.Buttons.Left) | MouseInput.ButtonPressed(MouseInput.Buttons.Right);
		if (justClicked)
		{
			_clickedInsideScene = isMouseOverSceneView;
		}

		AllowPassThroughEdges = false;

		HandleMouseScroll();
		bool validInput = (isMouseOverSceneView || _clickedInsideScene) && (_clickedInsideScene) || (justClicked == false && isMouseOverSceneView && _clickedInsideScene);
		if (MouseInput.IsButtonDown(MouseInput.Buttons.Left) || MouseInput.IsButtonDown(MouseInput.Buttons.Right))
		{
			if (validInput)
			{
				AllowPassThroughEdges = true;

				HandleButtonInputs();
			}
		}

		if (Camera.MainCamera.IsOrthographic == false)
		{
			if (validInput && MouseInput.IsButtonDown(MouseInput.Buttons.Right))
			{
				_smoothScreenDeltaVectorForMovement = Vector2.Lerp(_smoothScreenDeltaVectorForMovement, MouseInput.ScreenDelta, Time.EditorDeltaTime * 15);
			}
			else
			{
				_smoothScreenDeltaVectorForMovement = Vector2.Lerp(_smoothScreenDeltaVectorForMovement, Vector2.Zero, Time.EditorDeltaTime * 7);
			}

			if (validInput && MouseInput.IsButtonDown(MouseInput.Buttons.Left))
			{
				_smoothScreenDeltaVectorForRotation = Vector2.Lerp(_smoothScreenDeltaVectorForRotation, MouseInput.ScreenDelta, Time.EditorDeltaTime * 19);
			}
			else
			{
				_smoothScreenDeltaVectorForRotation = Vector2.Lerp(_smoothScreenDeltaVectorForRotation, Vector2.Zero, Time.EditorDeltaTime * 10);
			}

			MoveCameraByLocalVector(_smoothScreenDeltaVectorForMovement * 30 / Tofu.I.Window.WindowSize);
			Camera.MainCamera.Transform.Rotation += new Vector3(-_smoothScreenDeltaVectorForRotation.Y, _smoothScreenDeltaVectorForRotation.X, 0) * 0.2f;


			float keyboardMoveSpeed = _moveSpeed;

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
				//Camera.I.FieldOfView = Mathf.Lerp(Camera.I.FieldOfView, 100, Time.EditorDeltaTime * 7);
			}
			else
			{
				_moveSpeedMultiplier = Mathf.Lerp(_moveSpeedMultiplier, 1, Time.EditorDeltaTime * 10);
				//Camera.I.FieldOfView = Mathf.Lerp(Camera.I.FieldOfView, 60, Time.EditorDeltaTime * 7);
			}

			keyboardMoveSpeed = keyboardMoveSpeed * _moveSpeedMultiplier;

			_smoothKeyboardInputMoveVector = Vector3.Lerp(_smoothKeyboardInputMoveVector, _keyboardInputDirectionVector, Time.EditorDeltaTime * 4);

			MoveCameraByLocalVector(_smoothKeyboardInputMoveVector * keyboardMoveSpeed * Time.EditorDeltaTime);
		}
	}

	void HandleMouseScroll()
	{
		// Z POSITION
		if (MouseInput.ScrollDelta != 0)
		{
			if (Camera.MainCamera.IsOrthographic)
			{
				_targetOrthoSize += -MouseInput.ScrollDelta * (_targetOrthoSize * 0.04f);
				_targetOrthoSize = Mathf.Clamp(_targetOrthoSize, 0.1f, Mathf.Infinity);
				// Camera.I.ortographicSize = Mathf.Eerp(Camera.I.ortographicSize, targetOrthoSize, Time.editorDeltaTime * 10f);
				// macbook trackpad has smooth scrolling so no eerping
				Camera.MainCamera.OrthographicSize = _targetOrthoSize;
			}
			else
			{
				MoveCameraByLocalVector(new Vector3(0, 0, Mathf.Clamp(MouseInput.ScrollDelta, -10, 10)) * _moveSpeed*0.2f);

				//Camera.I.transform.position += Camera.I.transform.TransformDirection(Vector3.Forward) * MouseInput.ScrollDelta * 0.05f;
			}
		}
	}

	void HandleButtonInputs()
	{
		// PANNING
		if (MouseInput.IsButtonDown() && Camera.MainCamera.IsOrthographic)
		{
			Camera.MainCamera.Transform.LocalPosition += Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(new Vector2(-MouseInput.ScreenDelta.X, MouseInput.ScreenDelta.Y)) * Camera.MainCamera.OrthographicSize;
			// MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
		}
	}

	void MoveCameraByLocalVector(Vector3 moveVector)
	{
		Vector3 delta = Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(moveVector);

		// Debug.StatSetValue("DASDSAD", $"CameraDir:{Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(moveVector)}");
		//Debug.Log(delta);
		//Camera.I.Transform.LocalPosition += delta;
		Camera.MainCamera.Transform.WorldPosition += delta;
		// Camera.I.Transform.LocalPosition += dir * moveSpeed;

		Camera.MainCamera.UpdateMatrices();
	}
}