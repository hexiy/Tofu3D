using Tofu3D.Tweening;

namespace Tofu3D;

public class SceneNavigation
{
	bool _clickedInsideScene;
	float _targetOrthoSize = -1;

	public SceneNavigation()
	{
		I = this;
	}

	public static SceneNavigation I { get; private set; }

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
		if (_targetOrthoSize == -1 && Camera.I != null)
		{
			_targetOrthoSize = Camera.I.OrthographicSize;
		}

		if (TransformHandle.I.Clicked)
		{
			return;
		}

		if (Global.EditorAttached == false)
		{
			return;
		}

		if (KeyboardInput.WasKeyJustPressed(Keys.F))
		{
			MoveToGameObject(Editor.I.GetSelectedGameObject());
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
			if ((_clickedInsideScene) || (justClicked == false && isMouseOverSceneView && _clickedInsideScene))
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
				MoveCameraInDirection(new Vector3(0, 0, Mathf.ClampMax(MouseInput.ScrollDelta, 5)));

				//Camera.I.transform.position += Camera.I.transform.TransformDirection(Vector3.Forward) * MouseInput.ScrollDelta * 0.05f;
			}
		}

		// PANNING
		if (MouseInput.IsButtonDown())
		{
			//MoveCameraInDirection(new Vector2(-MouseInput.ScreenDelta.X, -MouseInput.ScreenDelta.Y));
			Camera.I.Transform.LocalPosition -= Camera.I.Transform.TransformDirection(new Vector2(MouseInput.ScreenDelta.X, MouseInput.ScreenDelta.Y)) / Units.OneWorldUnit * Camera.I.OrthographicSize;
			MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
		}


		if (MouseInput.IsButtonDown())
		{
			//Camera.I.Transform.Rotation += new Vector3(MouseInput.ScreenDelta.Y, MouseInput.ScreenDelta.X, 0) * 0.2f;
			//Camera.I.transform.Rotation = new Vector3(Camera.I.transform.Rotation.X,Camera.I.transform.Rotation .Y, 0);

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

			float moveSpeed = 0.4f;
			if (KeyboardInput.IsKeyDown(Keys.LeftShift))
			{
				moveSpeed = 0.04f;
			}

			MoveCameraInDirection(keyboardInputDirectionVector, moveSpeed);
		}
	}

	void MoveCameraInDirection(Vector3 dir, float moveSpeed = 0.02f)
	{
		Camera.I.Transform.LocalPosition += Camera.I.Transform.TransformDirection(dir) * moveSpeed;

		Camera.I.UpdateMatrices();
	}
}