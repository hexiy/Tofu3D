namespace Tofu3D;

[ExecuteInEditMode]
public class Camera : Component
{
	public static Action<Vector2> CameraSizeChanged = (newSize) => { };
	//public int antialiasingStrength = 0;
	public Color Color = new(34, 34, 34);
	public float NearPlaneDistance = 1;
	public float FarPlaneDistance = 50;
	[ShowIfNot(nameof(IsOrthographic))]
	public float FieldOfView = 2;
	public bool IsOrthographic = true;

	[ShowIf(nameof(IsOrthographic))]
	public float OrthographicSize = 2;

	//public float cameraSize = 0.1f;
	[XmlIgnore]
	public Matrix4x4 ProjectionMatrix;

	public Vector2 Size = new(1380, 900);
	[XmlIgnore]
	public Matrix4x4 TranslationMatrix;
	[XmlIgnore]
	public Matrix4x4 ViewMatrix;
	//[XmlIgnore] public RenderTarget2D renderTarget;

	public static Camera I { get; private set; }

	public override void Awake()
	{
		I = this;

		GameObject.AlwaysUpdate = true;
		// if (Global.EditorAttached == false)
		// {
		// 	Size = new Vector2(Tofu.I.Window.ClientSize.X, Tofu.I.Window.ClientSize.Y);
		// }

		ProjectionMatrix = GetProjectionMatrix();
		ViewMatrix = GetViewMatrix();
		TranslationMatrix = GetTranslationRotationMatrix();

		base.Awake();
	}

	public override void Start()
	{
		CameraSizeChanged.Invoke(Size);
		base.Start();
	}

	public override void Update()
	{
		if (IsOrthographic)
		{
			Transform.LocalScale = Vector3.One * OrthographicSize;
		}

		else
		{
			Transform.LocalScale = Vector3.One;
		}

		UpdateMatrices();
		base.Update();
	}

	public void SetSize(Vector2 newSize)
	{
		Size = newSize;

		CameraSizeChanged.Invoke(newSize);
	}

	public void UpdateMatrices()
	{
		ProjectionMatrix = GetProjectionMatrix();
		ViewMatrix = GetViewMatrix();
		TranslationMatrix = GetTranslationRotationMatrix();
	}

	Matrix4x4 GetViewMatrix()
	{
		//  const float radius = 500.0f;
		//  float camX = (float) Math.Sin(Time.EditorElapsedTime) * radius; //sin(glfwGetTime()) * radius;
		//  float camZ = (float) Math.Cos(Time.EditorElapsedTime) * radius; //cos(glfwGetTime()) * radius;
		//  float camY = (float) Math.Cos(Time.EditorElapsedTime) * radius; //cos(glfwGetTime()) * radius;
		//  Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(camX, camY, camZ), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
		// return view;


		Vector3 forwardWorld = Transform.WorldPosition + Transform.TransformDirectionToWorldSpace(new Vector3(0, 0, 1));
		Vector3 upLocal = Transform.TransformDirectionToWorldSpace(new Vector3(0, 1, 0));

		//Debug.Log($"Forward{forwardWorld}");
		//Debug.Log($"Up{upLocal}");
		Matrix4x4 view = Matrix4x4.CreateTranslation(Transform.WorldPosition * Units.OneWorldUnit * new Vector3(-1, -1, -1))
		               * Matrix4x4.CreateLookAt(cameraPosition: Transform.WorldPosition, cameraTarget: forwardWorld, cameraUpVector: upLocal)
			; // * Matrix4x4.CreateTranslation(Transform.WorldPosition * Units.OneWorldUnit * new Vector3(-1, -1, 1));
		return view;
	}

	Matrix4x4 GetProjectionMatrix()
	{
		if (IsOrthographic)
		{
			return GetOrthographicProjectionMatrix();
		}

		return GetPerspectiveProjectionMatrix();
	}

	Matrix4x4 GetPerspectiveProjectionMatrix()
	{
		FieldOfView = Mathf.ClampMin(FieldOfView, 0.0001f);
		NearPlaneDistance = Mathf.Clamp(NearPlaneDistance, 0.00001f, FarPlaneDistance);
		FarPlaneDistance = Mathf.Clamp(FarPlaneDistance, NearPlaneDistance + 0.001f, Mathf.Infinity);
		Matrix4x4 perspectiveMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), Size.X / Size.Y, NearPlaneDistance * Units.OneWorldUnit, FarPlaneDistance * Units.OneWorldUnit);

		// .CreatePerspective gives us great depth, but fieldofview doesnt?....
		return perspectiveMatrix;
	}

	Matrix4x4 GetOrthographicProjectionMatrix()
	{
		float left = -Size.X / 2;
		float right = Size.X / 2;
		float bottom = -Size.Y / 2;
		float top = Size.Y / 2;

		Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, NearPlaneDistance, FarPlaneDistance);

		return orthoMatrix * GetScaleMatrix();
	}

	Matrix4x4 GetTranslationRotationMatrix()
	{
		Matrix4x4 tr = Matrix4x4.CreateTranslation(-Transform.LocalPosition.X * Units.OneWorldUnit, -Transform.LocalPosition.Y * Units.OneWorldUnit, Transform.LocalPosition.Z * Units.OneWorldUnit);
		Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
		                                                            -Transform.Rotation.X / 180 * Mathf.Pi,
		                                                            -Transform.Rotation.Z / 180 * Mathf.Pi);
		return tr * rotationMatrix;
	}

	Matrix4x4 GetScaleMatrix()
	{
		Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(1 / OrthographicSize);
		return scaleMatrix;
	}

	public void Move(Vector2 moveVector)
	{
		Transform.WorldPosition += moveVector;
	}

	public Vector2 WorldToScreen(Vector2 worldPosition)
	{
		return Vector2.Transform(worldPosition, GetTranslationRotationMatrix());
	}

	public Vector2 ScreenToWorld(Vector2 screenPosition)
	{
		Vector3 worldPos;
		if (I.IsOrthographic)
		{
			worldPos = Vector3.Transform(screenPosition / Size * 2,
			                             Matrix.Invert(ProjectionMatrix))
			         - Size * OrthographicSize / 2;

			worldPos = worldPos / Units.OneWorldUnit;
		}

		else
		{
			worldPos = Vector3.Transform(screenPosition / Size * 2, Matrix.Invert(ProjectionMatrix));
			worldPos = worldPos / Units.OneWorldUnit;
		}

		// worldPos = Vector2.Transform(screenPosition / size * 2,Matrix.Invert(projectionMatrix)) - size;
		// worldPos = worldPos / Units.OneWorldUnit;

		//Debug.Log($"SCREEN:{screenPosition} | WORLD:{worldPos}");
		return worldPos;
	}

	public Vector2 CenterOfScreenToWorld()
	{
		return ScreenToWorld(new Vector2(Size.X / 2, Size.Y / 2));
	}

	public bool RectangleVisible(BoxShape shape)
	{
		bool isIn = Vector2.Distance(shape.Transform.WorldPosition, Transform.WorldPosition) < Size.X * 1.1f * (OrthographicSize / 2) + shape.Size.X / 2 * shape.Transform.LocalScale.MaxVectorMember();

		return isIn;
	}

	Matrix4x4 GetLightScaleMatrix()
	{
		Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(1 / OrthographicSize);
		return scaleMatrix;
	}

	public Matrix4x4 GetLightProjectionMatrix()
	{
		float left = -Size.X / 2;
		float right = Size.X / 2;
		float bottom = -Size.Y / 2;
		float top = Size.Y / 2;

		Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, NearPlaneDistance, FarPlaneDistance);

		return orthoMatrix * GetLightScaleMatrix();
	}

	public Matrix4x4 GetLightViewMatrix()
	{
		Vector3 oldRotation = Transform.Rotation;
		Transform.Rotation = -oldRotation;

		Vector3 forwardWorld = Transform.WorldPosition + Transform.TransformDirectionToWorldSpace(new Vector3(0, 0, 1));
		Vector3 upLocal = Transform.TransformDirectionToWorldSpace(new Vector3(0, 1, 0));


		//Debug.Log($"Forward{forwardWorld}");
		//Debug.Log($"Up{upLocal}");
		Matrix4x4 view = Matrix4x4.CreateTranslation(Transform.WorldPosition * Units.OneWorldUnit * new Vector3(-1, -1, 1))
		               * Matrix4x4.CreateLookAt(cameraPosition: Transform.WorldPosition, cameraTarget: forwardWorld, cameraUpVector: upLocal)
			; // * Matrix4x4.CreateTranslation(Transform.WorldPosition * Units.OneWorldUnit * new Vector3(-1, -1, 1));
		Transform.Rotation = oldRotation;

		return view;
	}
}