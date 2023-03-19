using Tofu3D.Tweening;

namespace Tofu3D;

public class SceneViewNavigation
{
	bool _clickedInsideScene;
	float _targetOrthoSize = -1;

	public SceneViewNavigation()
	{
		I = this;
	}

	public static SceneViewNavigation I { get; private set; }
	public bool IsPanningCamera { get; private set; }

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
		bool justClicked = MouseInput.ButtonPressed();
		if (justClicked)
		{
			_clickedInsideScene = isMouseOverSceneView;
		}

		if (isMouseOverSceneView || _clickedInsideScene)
		{
			if ((_clickedInsideScene) || (justClicked == false && isMouseOverSceneView ))//&& _clickedInsideScene))
			{
				HandleMouseControls();
			}
		}
	}

	void HandleMouseControls()
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

		// PANNING
		if (MouseInput.IsButtonDown() && Camera.I.IsOrthographic)
		{
			//MoveCameraInDirection(new Vector2(-MouseInput.ScreenDelta.X, -MouseInput.ScreenDelta.Y));
			Camera.I.Transform.LocalPosition -= Camera.I.Transform.TransformDirectionToWorldSpace(new Vector2(MouseInput.ScreenDelta.X, MouseInput.ScreenDelta.Y)) / Units.OneWorldUnit * Camera.I.OrthographicSize;
			MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
		}

		if (MouseInput.IsButtonDown(MouseInput.Buttons.Right) && Camera.I.IsOrthographic == false) // right click panning
		{
			//MoveCameraInDirection(new Vector2(-MouseInput.ScreenDelta.X, -MouseInput.ScreenDelta.Y));
			Camera.I.Transform.LocalPosition -= Camera.I.Transform.TransformDirectionToWorldSpace(new Vector2(MouseInput.ScreenDelta.X, MouseInput.ScreenDelta.Y)) / Units.OneWorldUnit * Camera.I.OrthographicSize;
			MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
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

			float moveSpeed = 10f;

			if (KeyboardInput.IsKeyDown(Keys.LeftShift))
			{
				moveSpeed = 20f;
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