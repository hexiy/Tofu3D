using Tofu3D.Tweening;

namespace Tofu3D;

public class SceneViewController
{
	bool _clickedInsideScene;
	float _targetOrthoSize = -1;

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

	// public ProjectionMode ProjectionMode
	// {
	// 	get { return (ProjectionMode) PersistentData.GetInt("SceneViewControllerPerspectiveMode", 0); }
	// 	private set { PersistentData.Set("SceneViewControllerPerspectiveMode", (int) value); }
	// }

	public void MoveToGameObject(GameObject targetGo)
	{
		Vector3 cameraStartPos = Camera.I.Transform.LocalPosition;
		Vector3 cameraEndPos = targetGo.Transform.LocalPosition + new Vector3(0, 0, -4);

		if (cameraStartPos == cameraEndPos)
		{
			cameraEndPos = targetGo.Transform.LocalPosition + new Vector3(0, 0, -2);
		}

		float cameraOrthoSize = Camera.I.OrthographicSize;
		Tweener.Tween(0, 1, 1.3f, progress =>
		{
			// Debug.Log("TWEENING:" + progress);
			Camera.I.OrthographicSize = cameraOrthoSize + (float) MathHelper.Sin(progress * Mathf.Pi) * 0.8f;
			Camera.I.Transform.LocalPosition = Vector3.Lerp(cameraStartPos, cameraEndPos, progress);
		});
	}

	// rotation before going into orthographic mode
	PersistentObject<Vector3> _cameraRotationInPerspectiveMode = ("_cameraRotationInPerspectiveMode", Vector3.Zero);
	PersistentObject<Vector3> _cameraPositionInPerspectiveMode = ("_cameraPositionInPerspectiveMode", Vector3.Zero);
	PersistentObject<float> _cameraFieldOfViewInperspectiveMode = ("_cameraFieldOfViewInperspectiveMode", 90);
	// PersistentObject<int> _savedInt = new PersistentObject<int>();

	public void SetProjectionMode(ProjectionMode newProjectionMode)
	{
		if (newProjectionMode == CurrentProjectionMode)
		{
			return;
		}

		if (newProjectionMode == ProjectionMode.Orthographic && CurrentProjectionMode == ProjectionMode.Perspective)
		{
			_cameraRotationInPerspectiveMode.Value = Camera.I.Transform.Rotation;
			_cameraPositionInPerspectiveMode.Value = Camera.I.Transform.WorldPosition;
			_cameraFieldOfViewInperspectiveMode.Value = Camera.I.FieldOfView;
		}

		float tweenDuration = 1f;
		Tweener.Tween(Camera.I.Transform.Rotation.X, newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.X : 0, tweenDuration, (f) => { Camera.I.Transform.Rotation = Camera.I.Transform.Rotation.Set(x: f); });
		Tweener.Tween(Camera.I.Transform.Rotation.Y, newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.Y : 0, tweenDuration, (f) => { Camera.I.Transform.Rotation = Camera.I.Transform.Rotation.Set(y: f); });
		Tweener.Tween(Camera.I.Transform.WorldPosition.Z, newProjectionMode == ProjectionMode.Perspective ? _cameraPositionInPerspectiveMode.Value.Z : Camera.I.Transform.WorldPosition.Z - 350, tweenDuration,
		              (f) => { Camera.I.Transform.WorldPosition = Camera.I.Transform.WorldPosition.Set(z: f); });

		Tween tween = Tweener.Tween(Camera.I.Transform.Rotation.Z, newProjectionMode == ProjectionMode.Perspective ? _cameraRotationInPerspectiveMode.Value.Z : 0, tweenDuration,
		                            (f) => { Camera.I.Transform.Rotation = Camera.I.Transform.Rotation.Set(z: f); });

		Tweener.Tween(Camera.I.FieldOfView, newProjectionMode == ProjectionMode.Perspective ? _cameraFieldOfViewInperspectiveMode : 14, tweenDuration, (f) => { Camera.I.FieldOfView = f; });


		if (newProjectionMode == ProjectionMode.Orthographic)
		{
			tween.SetOnComplete(() => { Camera.I.IsOrthographic = newProjectionMode == ProjectionMode.Orthographic; });
		}
		else
		{
			Camera.I.IsOrthographic = newProjectionMode == ProjectionMode.Orthographic;
		}

		CurrentProjectionMode.Value = newProjectionMode;
	}

	public void Update()
	{
		IsPanningCamera = false;
		if (_targetOrthoSize == -1 && Camera.I != null)
		{
			_targetOrthoSize = Camera.I.OrthographicSize;
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


		bool isMouseOverSceneView = MouseInput.ScreenPosition.X < Camera.I.Size.X && MouseInput.ScreenPosition.Y < Camera.I.Size.Y && MouseInput.ScreenPosition.Y > 0;
// Debug.Log($"isMouseOverSceneView:{isMouseOverSceneView}");
		bool justClicked = MouseInput.ButtonPressed(MouseInput.Buttons.Left) | MouseInput.ButtonPressed(MouseInput.Buttons.Right);
		if (justClicked)
		{
			_clickedInsideScene = isMouseOverSceneView;
		}

		AllowPassThroughEdges = false;

		HandleMouseScroll();
		if (MouseInput.IsButtonDown(MouseInput.Buttons.Left) || MouseInput.IsButtonDown(MouseInput.Buttons.Right))
		{
			if (isMouseOverSceneView || _clickedInsideScene)
			{
				if ((_clickedInsideScene) || (justClicked == false && isMouseOverSceneView && _clickedInsideScene))
				{
					AllowPassThroughEdges = true;

					HandleButtonInputs();
				}
			}
		}
	}

	void HandleMouseScroll()
	{
		// Z POSITION
		if (MouseInput.ScrollDelta != 0)
		{
			if (Camera.I.IsOrthographic)
			{
				_targetOrthoSize += -MouseInput.ScrollDelta * (_targetOrthoSize * 0.04f);
				_targetOrthoSize = Mathf.Clamp(_targetOrthoSize, 0.1f, Mathf.Infinity);
				// Camera.I.ortographicSize = Mathf.Eerp(Camera.I.ortographicSize, targetOrthoSize, Time.editorDeltaTime * 10f);
				// macbook trackpad has smooth scrolling so no eerping
				Camera.I.OrthographicSize = _targetOrthoSize;
			}
			else
			{
				MoveCameraInDirection(new Vector3(0, 0, Mathf.Clamp(MouseInput.ScrollDelta * 10, -10, 10)));

				//Camera.I.transform.position += Camera.I.transform.TransformDirection(Vector3.Forward) * MouseInput.ScrollDelta * 0.05f;
			}
		}
	}
	void HandleButtonInputs()
	{


		// PANNING
		if (MouseInput.IsButtonDown() && Camera.I.IsOrthographic)
		{
			Camera.I.Transform.LocalPosition -= Camera.I.Transform.TransformDirectionToWorldSpace(new Vector2(MouseInput.ScreenDelta.X, MouseInput.ScreenDelta.Y)) / Units.OneWorldUnit * Camera.I.OrthographicSize;
			// MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
		}

		if (MouseInput.IsButtonDown(MouseInput.Buttons.Right) && Camera.I.IsOrthographic == false) // right click panning
		{
			Camera.I.Transform.LocalPosition -= Camera.I.Transform.TransformDirectionToWorldSpace(new Vector2(MouseInput.ScreenDelta.X, MouseInput.ScreenDelta.Y)) / Units.OneWorldUnit * Camera.I.OrthographicSize;
			// MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
		}

		if (MouseInput.IsButtonDown() && Camera.I.IsOrthographic == false)
		{
			if (TransformHandle.I.CurrentAxisSelected != null)
			{
				return;
			}

			if (MouseInput.ScreenDelta != Vector2.Zero)
			{
				IsPanningCamera = true;
			}

			Camera.I.Transform.Rotation += new Vector3(MouseInput.ScreenDelta.Y, MouseInput.ScreenDelta.X, 0) * Time.EditorDeltaTime * 7;
			//Debug.Log("Rotate Cam");
			//Camera.I.transform.Rotation = new Vector3(Camera.I.transform.Rotation.X, Camera.I.transform.Rotation.Y, 0);

			Vector3 keyboardInputDirectionVector = Vector3.Zero;
			if (KeyboardInput.IsKeyDown(Keys.W))
			{
				keyboardInputDirectionVector += Vector3.Forward;
			}

			if (KeyboardInput.IsKeyDown(Keys.S))
			{
				keyboardInputDirectionVector += Vector3.Backward;
			}

			if (KeyboardInput.IsKeyDown(Keys.A))
			{
				keyboardInputDirectionVector += Vector3.Left;
			}

			if (KeyboardInput.IsKeyDown(Keys.D))
			{
				keyboardInputDirectionVector += Vector3.Right;
			}

			float moveSpeed = 30f;

			if (KeyboardInput.IsKeyDown(Keys.LeftShift))
			{
				moveSpeed = moveSpeed * 2;
				//Camera.I.FieldOfView = Mathf.Lerp(Camera.I.FieldOfView, 100, Time.EditorDeltaTime * 7);
			}
			else
			{
				//Camera.I.FieldOfView = Mathf.Lerp(Camera.I.FieldOfView, 60, Time.EditorDeltaTime * 7);
			}

			if (keyboardInputDirectionVector != Vector3.Zero)
			{
				MoveCameraInDirection(keyboardInputDirectionVector, moveSpeed);
			}
		}
	}

	void MoveCameraInDirection(Vector3 dir, float moveSpeed = 1f)
	{
		Vector3 delta = Camera.I.Transform.TransformDirectionToWorldSpace(dir) * moveSpeed * Time.EditorDeltaTime;

		//Debug.Log(delta);
		//Camera.I.Transform.LocalPosition += delta;
		Camera.I.Transform.WorldPosition += delta;
		// Camera.I.Transform.LocalPosition += dir * moveSpeed;

		Camera.I.UpdateMatrices();
	}
}